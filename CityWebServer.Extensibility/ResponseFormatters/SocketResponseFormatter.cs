using System;
using System.Net;

namespace CityWebServer.Extensibility.Responses {
	internal class SocketResponseFormatter: IResponseFormatter {
		private readonly HttpListenerRequest _request;

		//This GUID is specified by RFC 6455 and must be appended
		//to the socket key given by the client.
		private static readonly String KeyGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		public SocketResponseFormatter(HttpListenerRequest request) {
			_request = request;
		}

		public override void WriteContent(HttpListenerResponse response) {
			String key = _request.Headers.Get("Sec-WebSocket-Key");
			String respKey = Convert.ToBase64String(
				System.Security.Cryptography.SHA1.Create().ComputeHash(
					System.Text.Encoding.UTF8.GetBytes(key + KeyGuid)
				)
			);

			//response.Headers.Clear(); //no chunks
			response.SendChunked = false; //I said no chunks
			response.ContentLength64 = 0; //seriously, no chunks
			response.StatusCode = (int)HttpStatusCode.SwitchingProtocols;
			response.AddHeader("Connection", "Upgrade");
			response.AddHeader("Upgrade", "websocket");
			response.AddHeader("Sec-WebSocket-Accept", respKey);
			UnityEngine.Debug.Log($"WebSocket response key: {respKey}");

			byte[] dummy = new byte[1];
			response.OutputStream.Write(dummy, 0, 0);
		}
	}
}