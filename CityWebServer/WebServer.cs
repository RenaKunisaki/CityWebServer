using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using CityWebServer.Callbacks;
using CityWebServer.Extensibility;
using ColossalFramework.Plugins;
using ICities;
using JetBrains.Annotations;

namespace CityWebServer {
	[UsedImplicitly]
	public class WebServer: ThreadingExtensionBase, IWebServer, IAreasExtension,
	IDemandExtension, ITerrainExtension {
		protected static Stream logFile = null;
		protected static TextWriterTraceListener logListener;
		protected static String wwwRoot = null;
		private static string _endpoint; //used for UI button
		protected static FileWatcher fileWatcher = null;

		// Not required, but prevents a number of spurious entries from making it to the log file.
		private static readonly List<String> IgnoredAssemblies = new List<String> {
			"Anonymously Hosted DynamicMethods Assembly",
			"Assembly-CSharp",
			"Assembly-CSharp-firstpass",
			"Assembly-UnityScript-firstpass",
			"Boo.Lang",
			"ColossalManaged",
			"ICSharpCode.SharpZipLib",
			"ICities",
			"Mono.Security",
			"mscorlib",
			"System",
			"System.Configuration",
			"System.Core",
			"System.Xml",
			"UnityEngine",
			"UnityEngine.UI",
		};

		private List<IRequestHandler> _requestHandlers;
		private readonly TcpListener _listener;
		public IThreading threading;
		public CallbackList<FrameCallbackParam> frameCallbacks;
		public CallbackList<UnlockAreaCallbackParam> unlockAreaCallbacks;
		public CallbackList<UpdateDemandParam> updateDemandCallbacks;
		public CallbackList<TerrainCallbackParam> terrainCallbacks;


		/* So, why using TcpListener instead of HttpListener?
		 * Because, HttpListener closes the InputStream automatically
		 * after reading the request. There doesn't seem to be any way
		 * to prevent this.
		 * As a result, it's impossible to create a WebSocket connection.
		 * There is a `AcceptWebSocketAsync` method, but it's not showing
		 * up for me; I think MonoDevelop hasn't caught up to it yet.
		 * So because of that one little helpful feature, I've had to
		 * completely gut the server logic and reinvent the wheel, doing
		 * all my own header parsing, method handling, and response
		 * formatting and writing, just to have control over when the
		 * socket gets closed. Thanks, Microsoft!
		 */

		/// <summary>
		/// Gets the root endpoint for which the server is configured to service HTTP requests.
		/// </summary>
		public static String Endpoint {
			get { return _endpoint; }
		}

		public WebServer() {
			// We need a place to store all the request handlers that have been registered.
			_requestHandlers = new List<IRequestHandler>();
			frameCallbacks = new CallbackList<FrameCallbackParam>("Frame");
			unlockAreaCallbacks = new CallbackList<UnlockAreaCallbackParam>("UnlockArea");
			updateDemandCallbacks = new CallbackList<UpdateDemandParam>("updateDemand");
			terrainCallbacks = new CallbackList<TerrainCallbackParam>("Terrain");

			IPAddress address = IPAddress.Parse("127.0.0.1");
			int port = 7135;
			//_endpoint = $"http://{address.ToString()}:{port}/";
			_endpoint = "xxx";
			_listener = new TcpListener(address, port);
			//Log("Created Server");
		}

		/// <summary>
		/// Writes a message to the debug logs. "WebServer" tag,
		/// timestamp, and thread ID/name are automatically prepended.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void Log(String message) {
			String time = DateTime.Now.ToUniversalTime()
				.ToString("yyyyMMdd' 'HHmmss'.'fff");
			var tid = Thread.CurrentThread.ManagedThreadId;
			var tname = Thread.CurrentThread.Name;
			if(tname == "") tname = "?";
			message = $"{time} {tid}/{tname}: {message}{Environment.NewLine}";
			Trace.Write(message);
			Trace.Flush();
			try {
				UnityEngine.Debug.Log("[WebServer] " + message);
			}
			catch(NullReferenceException) {
				//Happens if Unity's logger isn't set up yet.
			}
		}


		#region User Methods
		/// <summary>
		/// Gets the full path to the directory where static pages are served from.
		/// </summary>
		public static String GetWebRoot() {
			if(wwwRoot != null) return wwwRoot;
			var modPaths = PluginManager.instance.GetPluginsInfo().Select(obj => obj.modPath);
			foreach(var path in modPaths) {
				var testPath = Path.Combine(path, "wwwroot");
				Log($"Trying path \"{testPath}\"...");
				if(Directory.Exists(testPath)) {
					Log($"Found wwwroot: \"{testPath}\"...");
					wwwRoot = testPath;
					return testPath;
				}
			}
			Log($"No wwwroot found!");
			throw new FileNotFoundException("WebServer: Can't find wwwroot directory!");
		}

		/// <summary>
		/// Begin serving requests.
		/// </summary>
		public void Run() {
			Log("Server starting");
			ThreadPool.QueueUserWorkItem(o => {
				Log("Server running");
				try {
					Thread.CurrentThread.Name = "WebServerMain";
				}
				catch(System.InvalidOperationException) {
					//Someone else already set the name; we can't set it again
				}
				_listener.Start();
				try {
					while(true) {
						//Wait for a client, and spawn a thread for it.
						TcpClient client = _listener.AcceptTcpClient();
						ThreadPool.QueueUserWorkItem(RequestProcessorCallback, client);
					}
				}
				catch(Exception ex) {
					//ignore these, just means socket closed 
					//because we're shutting down
					if(!(ex is System.Net.Sockets.SocketException)) {
						Log($"Error running server: {ex}");
						UnityEngine.Debug.LogException(ex);
					}
				}
			});
		}

		/// <summary>
		/// Stop serving new requests. Requests currently being served will
		/// still be finished.
		/// </summary>
		public void Stop() {
			_listener.Stop();
		}
		#endregion User Methods

		#region Request handlers

		/// <summary>
		/// Gets an array containing all currently registered request handlers.
		/// </summary>
		public IRequestHandler[] RequestHandlers {
			get { return _requestHandlers.ToArray(); }
		}

		/// <summary>
		/// Callback in the client handler thread.
		/// </summary>
		/// <param name="client">TcpClient; new client socket.</param>
		private void RequestProcessorCallback(object client) {
			TcpClient clnt = client as TcpClient;
			//clnt.ReceiveTimeout = 10000; //msec
			try {
				try {
					Thread.CurrentThread.Name = "WebServerRequestHandler";
				}
				catch(System.InvalidOperationException) {
					//Thread name can only be set once
				}
				var processor = new RequestProcessor(this, clnt);
				processor.Handle();
			}
			catch(Exception ex) {
				Log($"Error handling client: {ex}");
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Find a handler for this request.
		/// </summary>
		/// <returns>The handler.</returns>
		/// <param name="request">Request.</param>
		public IRequestHandler GetHandler(HttpRequest request) {
			return _requestHandlers.FirstOrDefault(obj => obj.ShouldHandle(request));
		}

		/// <summary>
		/// Searches all the assemblies in the current AppDomain for class definitions that implement the <see cref="IRequestHandler"/> interface.  Those classes are instantiated and registered as request handlers.
		/// </summary>
		private void RegisterHandlers() {
			IEnumerable<Type> handlers = FindHandlersInLoadedAssemblies();
			RegisterHandlers(handlers);
		}

		private void RegisterHandlers(IEnumerable<Type> handlers) {
			if(handlers == null) { return; }

			if(_requestHandlers == null) {
				_requestHandlers = new List<IRequestHandler>();
			}

			foreach(var handler in handlers) {
				// Only register handlers that we don't already have an instance of.
				Log($"Attempting to register handler: {handler.Name}");
				if(_requestHandlers.Any(h => h.GetType() == handler)) {
					Log($"Handler {handler.Name} is already registered");
					continue;
				}

				IRequestHandler handlerInstance = null;
				Boolean exists = false;

				//XXX this tries to register RequestHandlerBase itself,
				//which fails, adding a useless exception to the log.
				try {
					if(typeof(RequestHandlerBase).IsAssignableFrom(handler)) {
						handlerInstance = (RequestHandlerBase)Activator.CreateInstance(handler);
					}
					else {
						handlerInstance = (IRequestHandler)Activator.CreateInstance(handler);
					}

					if(handlerInstance == null) {
						Log($"Request Handler '{handler.Name}' could not be instantiated!");
						continue;
					}
					Log($"Handler {handler.Name} instantiated OK");

					// Duplicates handlers seem to pass the check above, so now we filter them based on their identifier values, which should work.
					exists = _requestHandlers.Any(obj => obj.HandlerID == handlerInstance.HandlerID);
				}
				catch(Exception ex) {
					Log($"Error instantiating handler '{handler.Name}': {ex.ToString()}");
					continue;
				}

				if(exists) {
					// TODO: Allow duplicate registrations to occur; previous registration is removed and replaced with a new one?
					Log($"Supressing duplicate handler registration for '{handler.Name}'");
				}
				else {
					_requestHandlers.Add(handlerInstance);
					Log($"Added Request Handler: {handler.FullName}");
				}
			}

			//Sort handlers by priority.
			_requestHandlers.OrderBy(handler => handler.Priority);

		}

		/// <summary>
		/// Searches all the assemblies in the current AppDomain, and returns a collection of those that implement the <see cref="IRequestHandler"/> interface.
		/// </summary>
		private static IEnumerable<Type> FindHandlersInLoadedAssemblies() {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach(var assembly in assemblies) {
				var handlers = FetchHandlers(assembly);
				foreach(var handler in handlers) {
					yield return handler;
				}
			}
		}

		private static IEnumerable<Type> FetchHandlers(Assembly assembly) {
			var assemblyName = assembly.GetName().Name;

			// Skip any assemblies that we don't anticipate finding anything in.
			if(IgnoredAssemblies.Contains(assemblyName)) { yield break; }

			Type[] types = new Type[0];
			try {
				types = assembly.GetTypes();
			}
			catch { }

			foreach(var type in types) {
				Boolean isValid = false;
				try {
					isValid = typeof(IRequestHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract;
				}
				catch { }

				if(isValid) {
					yield return type;
				}
			}
		}

		#endregion Request handlers


		#region ThreadingExtensionBase
		/// <summary>
		/// Called by the game after this instance is created.
		/// </summary>
		/// <param name="threading">The threading.</param>
		public override void OnCreated(IThreading threading) {
			this.threading = threading;
			if(logFile == null) {
				logFile = File.Create("CSL-WebServer.log");
				logListener = new TextWriterTraceListener(logFile);
				Trace.Listeners.Add(logListener);
			}
			Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			Log("Initializing...");

			if(fileWatcher == null) {
				fileWatcher = new FileWatcher();
			}

			GetWebRoot();
			try {
				RegisterHandlers();
			}
			catch(Exception ex) {
				Log($"Error registering handlers: {ex}");
				UnityEngine.Debug.LogException(ex);
			}

			Run();
			base.OnCreated(threading);
		}

		/// <summary>
		/// Called by the game before this instance is about to be destroyed.
		/// </summary>
		public override void OnReleased() {
			Log("Shutting down.");
			if(_listener != null) _listener.Stop();

			// TODO: Unregister from events
			if(_requestHandlers != null) _requestHandlers.Clear();
			base.OnReleased();
		}

		/// <summary>
		/// Called once per rendered frame.
		/// Thread: Main
		/// </summary>
		/// <param name="realTimeDelta">Seconds since previous frame.</param>
		/// <param name="simulationTimeDelta">Smoothly interpolated to be used
		/// from main thread. On normal speed it is roughly same as realTimeDelta.</param>
		public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
			//Log($"Start frame callbacks, dt={realTimeDelta}, {simulationTimeDelta}");
			frameCallbacks.Call(new FrameCallbackParam {
				realTimeDelta = realTimeDelta,
				simulationTimeDelta = simulationTimeDelta,
			});
		}

		#endregion ThreadingExtensionBase

		#region IAreasExtension

		/// <summary>
		/// Invoked when the extension initializes.
		/// Thread: Main
		/// For more info on IAreas, see #IAreas.
		/// We don't use this because we already have another OnCreated method.
		/// </summary>
		/// <param name="areas">Areas.</param>
		public void OnCreated(IAreas areas) { }

		/// <summary>
		/// Invoked when the game checks if a tile can be unlocked
		/// Thread: Any
		/// </summary>
		/// <returns><c>true</c>, if can unlock area, <c>false</c> otherwise.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		/// <param name="originalResult">Original result from game.</param>
		public bool OnCanUnlockArea(int x, int z, bool originalResult) {
			return originalResult;
		}

		/// <summary>
		/// Invoked when the game calculates the price of a tile.
		/// Thread: Any
		/// </summary>
		/// <returns>The area price.</returns>
		/// <param name="ore">Amount of Ore.</param>
		/// <param name="oil">Amount of Oil.</param>
		/// <param name="forest">Amount of Forest.</param>
		/// <param name="fertility">Amount of Fertile Land.</param>
		/// <param name="water">Amount of Water.</param>
		/// <param name="road">Whether roads are present.</param>
		/// <param name="train">Whether railways are present.</param>
		/// <param name="ship">Whether ship paths are present.</param>
		/// <param name="plane">Whether plane paths are present.</param>
		/// <param name="landFlatness">Land flatness.</param>
		/// <param name="originalPrice">Original price from game.</param>
		public int OnGetAreaPrice(uint ore, uint oil, uint forest,
		uint fertility, uint water, bool road, bool train, bool ship,
		bool plane, float landFlatness, int originalPrice) {
			return originalPrice;
		}

		/// <summary>
		/// Invoked when the game unlocks a tile.
		/// Thread: Simulation
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="z">The z coordinate.</param>
		public void OnUnlockArea(int x, int z) {
			unlockAreaCallbacks.Call(new UnlockAreaCallbackParam { x = x, z = z });
		}

		#endregion IAreasExtension

		#region IDemandExtension

		public void OnCreated(IDemand demand) { }

		public int OnCalculateResidentialDemand(int originalDemand) {
			updateDemandCallbacks.Call(new UpdateDemandParam {
				demand = originalDemand, which = 'R'
			});
			return originalDemand;
		}

		public int OnCalculateCommercialDemand(int originalDemand) {
			updateDemandCallbacks.Call(new UpdateDemandParam {
				demand = originalDemand, which = 'C'
			});
			return originalDemand;
		}

		public int OnCalculateWorkplaceDemand(int originalDemand) {
			updateDemandCallbacks.Call(new UpdateDemandParam {
				demand = originalDemand, which = 'W'
			});
			return originalDemand;
		}

		public int OnUpdateDemand(int lastDemand, int nextDemand, int targetDemand) { return lastDemand; }

		#endregion IDemandExtension


		#region ITerrainExtension

		public void OnCreated(ITerrain terrain) { }

		/// <summary>
		/// Invoked after the terrain heights have been modified
		/// Thread: Simulation
		/// </summary>
		/// <param name="minX">Minimum X coord of modification.</param>
		/// <param name="minZ">Minimum Z coord of modification.</param>
		/// <param name="maxX">Maximum X coord of modification.</param>
		/// <param name="maxZ">Maximum Z coord of modification.</param>
		public void OnAfterHeightsModified(float minX, float minZ, float maxX, float maxZ) {
			terrainCallbacks.Call(new TerrainCallbackParam {
				minX = minX,
				minZ = minZ,
				maxX = maxX,
				maxZ = maxZ,
			});
		}

		#endregion ITerrainExtension
	}
}