using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CityWebServer.Extensibility;
using JetBrains.Annotations;
using ICities;
using System.Collections.Generic;
using ColossalFramework.Plugins;
using System.Linq;

namespace CityWebServer {
	[UsedImplicitly]
	public class WebServer: ThreadingExtensionBase, IWebServer {
		private List<IRequestHandler> _requestHandlers;
		private readonly TcpListener _listener;
		protected static Stream logFile = null;
		protected static TextWriterTraceListener logListener;
		protected static String wwwRoot = null;
		private static string _endpoint; //used for UI button

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
			catch {
				//it's possible the logger isn't set up yet...
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
			Log("Initializing...");
			/* try {
				RegisterHandlers();
			}
			catch(Exception ex) {
				UnityEngine.Debug.LogException(ex);
			} */

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

		/// <summary>
		/// Gets an array containing all currently registered request handlers.
		/// </summary>
		public IRequestHandler[] RequestHandlers {
			get { return _requestHandlers.ToArray(); }
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
						ThreadPool.QueueUserWorkItem(RequestHandlerCallback, client);
					}
				}
				catch(Exception ex) {
					Log($"Error running server: {ex}");
					UnityEngine.Debug.LogException(ex);
				}
			});
		}

		private void RequestHandlerCallback(object client) {
			//Callback in the client handler thread.
			try {
				var handler = new RequestHandler(this, client as TcpClient);
				handler.Handle();
			}
			catch(Exception ex) {
				Log($"Error handling client: {ex}");
				UnityEngine.Debug.LogException(ex);
			}
		}

		public void Stop() {
			_listener.Stop();
		}
	}
}