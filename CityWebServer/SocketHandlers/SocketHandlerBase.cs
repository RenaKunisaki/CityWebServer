using System.Collections.Generic;
using CityWebServer.RequestHandlers;

namespace CityWebServer.SocketHandlers {
	public class SocketHandlerBase {
		/** Base class for WebSocket handlers.
		 *  These are instantiated by SocketRequestHandler and can enqueue
		 *  messages to be sent to the client.
		 *  XXX they should also be able to respond to incoming messages.
		 */
		protected SocketRequestHandler handler;
		protected string Name;

		public SocketHandlerBase(SocketRequestHandler handler, string name) {
			this.handler = handler;
			this.Name = name;
		}

		public void SendJson<T>(T body, string name = null) {
			/** Encode an object as JSON and send it to the client.
			 *  This method encapsulates the object in {name:object},
			 *  where the default name is the handler name.
			 *  This allows the client to know what kind of message
			 *  it's dealing with.
			 */
			if(name == null) name = Name;
			SendUnencapsulatedJson(new Dictionary<string, T> {
				{ name, body},
			});
		}


		public void SendUnencapsulatedJson<T>(T body) {
			/** Encode an object as JSON and send it to the client,
			 *  without encapsulating it in {name:object}.	
			 */
			var writer = new JsonFx.Json.JsonWriter();
			handler.EnqueueMessage(writer.Write(body));
		}

		protected void Log(string msg) {
			WebServer.Log($"Socket.{Name}: {msg}");
		}
	}
}