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

		public class HttpRequest {
			public String method;
			public String path;
			public String version;
			public Dictionary<String, String> headers;
		}

		public RequestHandler(WebServer server, TcpClient client) {
			this.server = server;
			this.client = client;
			this.stream = client.GetStream();
		}

		public void Log(String message) {
			WebServer.Log(message);
		}

		protected void Send(String message) {
			byte[] reply = System.Text.Encoding.UTF8.GetBytes(message);
			stream.Write(reply, 0, reply.Length);
		}

		public void Handle() {
			try {
				Log($"Handling connection from {client.Client.RemoteEndPoint}");
				HttpRequest req = GetRequest(out String body);
				Log($"Request: method={req.method} path={req.path} ver={req.version}");
				foreach(KeyValuePair<String, String> header in req.headers) {
					Log($"Request header '{header.Key}' = '{header.Value}'");
				}
				//Log($"Request body = '{body}'");

				Send(
					"HTTP/1.1 200 OK\r\n" +
					"Content-Type: text/plain\r\n" +
					"\r\n" +
					"hello");
				Log($"Done handling client {client.Client.RemoteEndPoint}");
			}
			finally {
				stream.Close();
				client.Close();
			}
		}

		protected HttpRequest GetRequest(out String leftovers) {
			//Gotta parse it all by hand because we can't use HttpListener
			//because it helpfully closes the socket for us making it
			//impossible to accept a WebSocket connection
			//and we can't use AcceptWebSocketAsync because I guess Mono
			//is behind the times and doesn't support that.
			byte[] buffer = new byte[1024];
			String message = "";

			//TODO we should limit the length of a line, the number of
			//lines we'll accept, and how long we'll wait for them,
			//so that buggy/malicious clients don't break everything.

			//Get first line
			//Log("Getting request line");
			while(!message.Contains("\r\n")) {
				int r = stream.Read(buffer, 0, buffer.Length);
				if(r <= 0) { //socket closed
					throw new OperationCanceledException();
				}
				message += System.Text.Encoding.UTF8.GetString(buffer);
			}
			//Log($"Request line is '{message}'");

			//Terrible parsing, wow is C# string manipulation ever bad
			String[] shit = new String[1];
			shit[0] = "\r\n";
			List<String> lines = new List<String>(
				message.Split(shit, StringSplitOptions.None));
			String[] parts = lines[0].Split(' ');
			lines.RemoveAt(0);

			var req = new HttpRequest {
				method = parts[0],
				path = parts[1],
				version = parts[2],
				headers = new Dictionary<string, string>(),
			};

			//Get headers
			//Log("Getting headers");
			message = String.Join("\r\n", lines.ToArray());
			while(!message.Contains("\r\n\r\n")) {
				int r = stream.Read(buffer, 0, buffer.Length);
				if(r <= 0) { //socket closed
					throw new OperationCanceledException();
				}
				message += System.Text.Encoding.UTF8.GetString(buffer);
			}
			//Log($"Header buffer is '{message}'");

			//Parse headers
			//Split the headers from whatever came after them
			String[] shit3 = new String[1];
			shit3[0] = "\r\n\r\n";
			String[] shit4 = message.Split(shit3, StringSplitOptions.None);
			leftovers = shit4[1]; //anything after is request body

			//Split the headers at "\r\n"
			List<String> headerLines = new List<String>(
				shit4[0].Split(shit, StringSplitOptions.None));

			//Split each header at ": "
			String[] shit2 = new String[1];
			shit2[0] = ": ";
			foreach(String line in headerLines) {
				if(line == "") break;
				String[] vs = line.Split(shit2, 2, StringSplitOptions.None);
				//Log($"HTTP header '{vs[0]}' = '{vs[1]}'");
				req.headers[vs[0].ToLower()] = vs[1];
			}

			//Log("Got HTTP request.");
			return req;
		}
	}
}