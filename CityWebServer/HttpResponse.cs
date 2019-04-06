using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace CityWebServer {
	public class HttpResponse {
		public int statusCode = 200;
		public String version = "HTTP/1.1";
		public Dictionary<String, String> headers;
		protected Stream stream;
		protected bool sentHeaders = false;

		public HttpResponse(Stream stream, String contentType = null, int statusCode = 200) {
			this.stream = stream;
			this.statusCode = statusCode;
			this.headers = new Dictionary<string, string>();
			if(contentType != null) headers["Content-Type"] = contentType;
		}

		public void AddHeader(String name, String value) {
			this.headers[name] = value;
		}

		public void SendHeaders() {
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(this.BuildResponseText());
			stream.Write(msg, 0, msg.Length);
			sentHeaders = true;
		}

		public void SendBody(String body) {
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(body);
			SendBody(msg);
		}

		public void SendBody(byte[] body) {
			if(!sentHeaders) {
				headers["Content-Length"] = $"{body.Length}";
				SendHeaders();
			}
			stream.Write(body, 0, body.Length);
		}

		public void SendPartialBody(String body) {
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(body);
			stream.Write(msg, 0, msg.Length);
		}

		public void SendPartialBody(byte[] body) {
			stream.Write(body, 0, body.Length);
		}

		public String BuildResponseText() {
			String statusText = Enum.GetName(
				typeof(System.Net.HttpStatusCode), statusCode);
			String response = $"{version} {statusCode} {statusText}\r\n";
			foreach(KeyValuePair<String, String> header in headers) {
				response += $"{header.Key}: {header.Value}\r\n";
			}
			return response + "\r\n";
		}
	}
}