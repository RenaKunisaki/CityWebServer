using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.IO;
using ApacheMimeTypes;
using System.Runtime.InteropServices;

namespace CityWebServer {
	public class RequestProcessor {
		protected WebServer server;
		protected TcpClient client;
		protected NetworkStream stream;

		public RequestProcessor(WebServer server, TcpClient client) {
			this.server = server;
			this.client = client;
			this.stream = client.GetStream();
		}

		public void Log(String message) {
			WebServer.Log(message);
		}

		protected void Send(String message) {
			//Helpful wrapper to send raw string to client.
			byte[] reply = System.Text.Encoding.UTF8.GetBytes(message);
			stream.Write(reply, 0, reply.Length);
		}

		public void Handle() {
			/** Called to handle a client connection.
			 */
			try {
				//Log($"Handling connection from {client.Client.RemoteEndPoint}");
				HttpRequest req = new HttpRequest().Read(stream, out String body);
				//Log($"Request: method={req.method} path={req.path} ver={req.version}");
				//foreach(KeyValuePair<String, String> header in req.headers) {
				//	Log($"Request header '{header.Key}' = '{header.Value}'");
				//}
				//Log($"Request body = '{body}'");

				var handler = server.GetHandler(req);
				if(handler != null) {
					try {
						//Log($"Using handler '{handler.Name}' for {req.method} {req.path} from {client.Client.RemoteEndPoint}");
						//Create a new instance of the handler to deal with
						//this request, so that they don't stomp on eachother
						//when multiple threads are involved.
						IRequestHandler instance = (IRequestHandler)Activator.CreateInstance(handler.GetType(),
							new object[] { server, req, handler.Name });
						instance.Handle();
					}
					catch(Exception ex) {
						if(ex is System.IO.IOException) {
							//probably the client closed the socket
							Log($"IO error (probably harmless) in handler {handler.Name} for {req.method} {req.path}: {ex.Message}");
						}
						else {
							Log($"Error in handler {handler.Name} for {req.method} {req.path} with client {client.Client.RemoteEndPoint}: {ex}");
							SendErrorResponse(req, HttpStatusCode.InternalServerError, ex.ToString());
						}
					}
					//Log($"Done handling client {client.Client.RemoteEndPoint}");
					return;
				}
				Log($"No handler found for {req.method} {req.path}");
				SendErrorResponse(req, HttpStatusCode.InternalServerError,
					"No handler responded to this request");
			}
			finally {
				Finish();
			}
		}

		protected void Finish() {
			/** Clean up after handling client connection.
			 */
			stream.Close();
			client.Close();
		}

		public void SendErrorResponse(HttpRequest request, HttpStatusCode status,
		String message = null) {
			/** Send an error resposne to the client.
			 */
			var resp = new HttpResponse(stream,
				contentType: "text/plain",
				statusCode: status);
			if(message == null) {
				message = HttpResponse.StatusCodeToName(status);
			}
			resp.SendBody(message);
		}
	}
}