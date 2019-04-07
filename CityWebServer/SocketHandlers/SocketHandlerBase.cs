using System.IO;
using System.Net.Sockets;
using System.Text;
using CityWebServer.RequestHandlers;

namespace CityWebServer {
	public class SocketHandlerBase {
		protected SocketRequestHandler handler;
		protected string Name;

		public SocketHandlerBase(SocketRequestHandler handler, string name) {
			this.handler = handler;
			this.Name = name;
		}

		public void SendJson<T>(T body) {
			var writer = new JsonFx.Json.JsonWriter();
			handler.EnqueueMessage(writer.Write(body));
		}

		protected void Log(string msg) {
			WebServer.Log($"Socket.{Name}: {msg}");
		}
	}
}