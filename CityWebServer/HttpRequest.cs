using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace CityWebServer {
	public class HttpRequest {
		public String method;
		public String path;
		public String version;
		public Dictionary<String, String> headers;
		protected byte[] buffer;
		protected String message = "";
		public NetworkStream stream;
		//These are arrays because lol C#
		protected static readonly String[] _split_crlf = { "\r\n" };
		protected static readonly String[] _split_crlf2 = { "\r\n\r\n" };

		public HttpRequest() {
			buffer = new byte[1024];
		}

		public HttpRequest Read(NetworkStream stream, out String body) {
			/** Read the request from a stream.
			 *  <param name="stream">Stream to read from.</param>
			 *  <param name="body">Receives request body, if any.</param>
			 *  <returns>Itself.</returns>
			 */

			//TODO we should limit the length of a line, the number of
			//lines we'll accept, and how long we'll wait for them,
			//so that buggy/malicious clients don't break everything.

			this.stream = stream;
			GetFirstLine();
			body = GetHeaders();
			return this;
		}

		protected String ReadUntil(String delimiter) {
			while(!message.Contains(delimiter)) {
				int r = stream.Read(buffer, 0, buffer.Length);
				if(r <= 0) { //socket closed
					throw new OperationCanceledException();
				}
				//XXX what if we didn't receive a full code point?
				message += System.Text.Encoding.UTF8.GetString(buffer);
			}
			return message;
		}

		protected void GetFirstLine() {
			//Log("Getting request line");
			ReadUntil("\r\n");
			//Log($"Request line is '{message}'");

			//Terrible parsing, wow is C# string manipulation ever bad
			List<String> lines = new List<String>(
				message.Split(_split_crlf, StringSplitOptions.None));
			String[] parts = lines[0].Split(' ');
			lines.RemoveAt(0);

			this.method = parts[0].ToUpper();
			this.path = parts[1];
			this.version = parts[2];

			//put the rest back into the buffer
			message = String.Join("\r\n", lines.ToArray());
		}

		protected String GetHeaders() {
			headers = new Dictionary<string, string>();

			ReadUntil("\r\n\r\n");
			//Log($"Header buffer is '{message}'");

			//Parse headers
			//Split the headers from whatever came after them
			String[] parts = message.Split(_split_crlf2, StringSplitOptions.None);

			//Split the headers at "\r\n"
			List<String> headerLines = new List<String>(
				parts[0].Split(_split_crlf, StringSplitOptions.None));

			//Split each header at ": "
			String[] shit2 = new String[1];
			shit2[0] = ": ";
			foreach(String line in headerLines) {
				if(line == "") break;
				String[] vs = line.Split(shit2, 2, StringSplitOptions.None);
				//Log($"HTTP header '{vs[0]}' = '{vs[1]}'");
				this.headers[vs[0].ToLower()] = vs[1];
			}

			return parts[1]; //anything after is request body
		}
	}
}