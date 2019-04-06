using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace CityWebServer {
	public class RequestHandler {
		protected WebServer server;
		protected TcpClient client;
		protected NetworkStream stream;

		public RequestHandler(WebServer server, TcpClient client) {
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
				Log($"Handling connection from {client.Client.RemoteEndPoint}");
				HttpRequest req = new HttpRequest().Read(stream, out String body);
				Log($"Request: method={req.method} path={req.path} ver={req.version}");
				foreach(KeyValuePair<String, String> header in req.headers) {
					Log($"Request header '{header.Key}' = '{header.Value}'");
				}
				//Log($"Request body = '{body}'");

				var resp = new HttpResponse(stream, contentType: "text/plain");
				resp.SendBody("hello");
				Log($"Done handling client {client.Client.RemoteEndPoint}");
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
	}
}