using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace CityWebServer {
	public class HttpResponse {
		public HttpStatusCode statusCode;
		public String version = "HTTP/1.1";
		public Dictionary<String, String> headers;
		protected Stream stream;
		protected bool sentHeaders = false;

		public HttpResponse(Stream stream, String contentType = null,
		HttpStatusCode statusCode = HttpStatusCode.OK) {
			this.stream = stream;
			this.statusCode = statusCode;
			this.headers = new Dictionary<string, string>();
			if(contentType != null) AddHeader("Content-Type", contentType);
		}

		public static String StatusCodeToName(HttpStatusCode code) {
			return Regex.Replace(code.ToString(),
				"(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
		}

		public void AddHeader(String name, String value) {
			this.headers[name] = value;
		}

		public void SendHeaders() {
			if(sentHeaders) return;
			WebServer.Log($"Sending HTTP response, status:{statusCode}");
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
			SendHeaders();
			byte[] msg = System.Text.Encoding.UTF8.GetBytes(body);
			stream.Write(msg, 0, msg.Length);
		}

		public void SendPartialBody(byte[] body) {
			SendHeaders();
			stream.Write(body, 0, body.Length);
		}

		public void SendPartialBody(byte[] body, int offset, int length) {
			SendHeaders();
			stream.Write(body, offset, length);
		}

		public String BuildResponseText() {
			String statusText = StatusCodeToName(statusCode);
			int code = (int)statusCode;
			String response = $"{version} {code} {statusText}\r\n";
			foreach(KeyValuePair<String, String> header in headers) {
				response += $"{header.Key}: {header.Value}\r\n";
			}
			return response + "\r\n";
		}
	}
}