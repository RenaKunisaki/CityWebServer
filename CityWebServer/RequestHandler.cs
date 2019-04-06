﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.IO;
using ApacheMimeTypes;

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

				if(req.method == "GET") HandleGet(req);
				else {
					SendErrorResponse(req, HttpStatusCode.MethodNotAllowed);
				}
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

		public void HandleGet(HttpRequest request) {
			/** Handle a GET request.
			 */
			String path = request.path;
			Log($"Handling GET path '{path}");
			path = path.Replace("..", "%2E%2E");
			while(path.StartsWith("/", StringComparison.Ordinal)) {
				path = path.Substring(1);
			}
			if(path == "") path = "index.html";
			path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
			String root = WebServer.GetWebRoot();
			var absolutePath = Path.Combine(root, path);
			var extension = Path.GetExtension(absolutePath);
			Log($"Sending file '{absolutePath}'; root='{root}' path='{path}'");

			try {
				var contentType = Apache.GetMime(extension);
				var resp = new HttpResponse(stream);
				if(contentType != null) resp.AddHeader("Content-Type", contentType);

				using(FileStream fileReader = File.OpenRead(absolutePath)) {
					byte[] buffer = new byte[4096];
					int read;
					while((read = fileReader.Read(buffer, 0, buffer.Length)) > 0) {
						resp.SendPartialBody(buffer, 0, read);
					}
				}
			}
			catch(System.IO.FileNotFoundException) {
				Log($"Not found: '{absolutePath}'");
				SendErrorResponse(request, HttpStatusCode.NotFound);
			}
			catch(Exception ex) {
				Log($"Error sending '{absolutePath}': {ex}");
				SendErrorResponse(request,
					HttpStatusCode.InternalServerError, ex.ToString());
			}
		}
	}
}