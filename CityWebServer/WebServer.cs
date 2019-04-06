using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using CityWebServer.Extensibility;
using ColossalFramework.Plugins;
using ICities;
using JetBrains.Annotations;

namespace CityWebServer {
	[UsedImplicitly]
	public class WebServer: ThreadingExtensionBase, IWebServer {
		private List<IRequestHandler> _requestHandlers;
		private readonly TcpListener _listener;
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

			IPAddress address = IPAddress.Parse("127.0.0.1");
			int port = 7135;
			//_endpoint = $"http://{address.ToString()}:{port}/";
			_endpoint = "xxx";
			_listener = new TcpListener(address, port);
			//Log("Created Server");
		}

		public static void Log(String message) {
			String time = DateTime.Now.ToUniversalTime()
				.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fff");
			message = $"{time}: {message}{Environment.NewLine}";
			Trace.Write(message);
			Trace.Flush();
			try {
				UnityEngine.Debug.Log("[WebServer] " + message);
			}
			catch(NullReferenceException) {
				//Happens if Unity's logger isn't set up yet.
			}
		}

		#region Create
		/// <summary>
		/// Called by the game after this instance is created.
		/// </summary>
		/// <param name="threading">The threading.</param>
		public override void OnCreated(IThreading threading) {
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
		#endregion Create

		#region Release
		/// <summary>
		/// Called by the game before this instance is about to be destroyed.
		/// </summary>
		public override void OnReleased() {
			Log("Shutting down.");
			if(_listener != null) _listener.Stop();

			// TODO: Unregister from events (i.e. ILogAppender.LogMessage)
			if(_requestHandlers != null) _requestHandlers.Clear();
			base.OnReleased();
		}
		#endregion Release


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
			return null;
		}

		public void Run() {
			Log("Server starting");
			ThreadPool.QueueUserWorkItem(o => {
				Log("Server running");
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

		public void Stop() {
			_listener.Stop();
		}
		#endregion User Methods

		/// <summary>
		/// Gets an array containing all currently registered request handlers.
		/// </summary>
		public IRequestHandler[] RequestHandlers {
			get { return _requestHandlers.ToArray(); }
		}

		private void RequestProcessorCallback(object client) {
			//Callback in the client handler thread.
			try {
				var processor = new RequestProcessor(this, client as TcpClient);
				processor.Handle();
			}
			catch(Exception ex) {
				Log($"Error handling client: {ex}");
				UnityEngine.Debug.LogException(ex);
			}
		}

		public IRequestHandler GetHandler(HttpRequest request) {
			return _requestHandlers.FirstOrDefault(obj => obj.ShouldHandle(request));
		}

		#region Built-in Handlers

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

				try {
					if(typeof(RequestHandlerBase).IsAssignableFrom(handler)) {
						handlerInstance = (RequestHandlerBase)Activator.CreateInstance(handler, this);
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

		#endregion Built-in Handlers
	}
}