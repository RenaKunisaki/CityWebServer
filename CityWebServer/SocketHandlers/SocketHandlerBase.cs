using System.Collections.Generic;
using System.Net;
using CityWebServer.RequestHandlers;

namespace CityWebServer.SocketHandlers {
	public class SocketHandlerBase {
		/** Base class for WebSocket handlers.
		 *  These are instantiated by SocketRequestHandler and can enqueue
		 *  messages to be sent to the client.
		 *  XXX they should also be able to respond to incoming messages.
		 */
		protected SocketRequestHandler handler;
		protected WebServer server;
		protected string Name;

		public SocketHandlerBase(SocketRequestHandler handler, string name) {
			this.handler = handler;
			this.server = handler.Server as WebServer;
			this.Name = name;
		}

		/// <summary>
		/// Encode an object as JSON and send it to the client.
		/// </summary>
		/// <param name="body">Object to send.</param>
		/// <param name="name">Class name. Default is the name given
		/// in the handler's constructor.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <remarks>This method encapsulates the object in {name:object},
		/// where the default name is the handler name.
		/// This allows the client to know what kind of message
		/// it's dealing with.</remarks>
		public void SendJson<T>(T body, string name = null) {
			if(name == null) name = Name;
			SendUnencapsulatedJson(new Dictionary<string, T> {
				{name, body},
			});
		}

		/// <summary>
		/// Encode an object as JSON and send it to the client,
		/// without encapsulating it in {name:object}.	
		/// </summary>
		/// <param name="body">Object to send.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void SendUnencapsulatedJson<T>(T body) {
			var writer = new JsonFx.Json.JsonWriter();
			handler.EnqueueMessage(writer.Write(body));
		}

		/// <summary>
		/// Send an error response.
		/// </summary>
		/// <param name="error">Error message.</param>
		/// <param name="name">Class name. Default is the name given
		/// in the handler's constructor.</param>
		public void SendErrorResponse(string error, string name = null) {
			if(name == null) name = Name;
			SendUnencapsulatedJson(new Dictionary<string, Dictionary<string, string>> {
				{"error", new Dictionary<string, string> {
					{name, error},
				}},
			});
		}

		public void SendErrorResponse(HttpStatusCode code, string name = null) {
			SendErrorResponse(code.ToString(), name);
		}

		protected void Log(string msg) {
			WebServer.Log($"Socket.{Name}: {msg}");
		}
	}
}