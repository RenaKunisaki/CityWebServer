using System;
using System.Net;

namespace CityWebServer.Extensibility.Responses {
	internal class BinaryResponseFormatter: IResponseFormatter {
		private readonly byte[] _content;
		private readonly HttpStatusCode _statusCode;

		public BinaryResponseFormatter(byte[] content, HttpStatusCode statusCode) {
			_content = content;
			_statusCode = statusCode;
		}

		public override void WriteContent(HttpListenerResponse response) {
			response.StatusCode = (int)_statusCode;
			response.ContentType = "application/octet-stream";
			response.ContentLength64 = _content.Length;
			response.OutputStream.Write(_content, 0, _content.Length);
		}
	}
}