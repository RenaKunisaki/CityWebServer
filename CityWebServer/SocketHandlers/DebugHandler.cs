using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using CityWebServer.Callbacks;
using System.Linq;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends debug info to client.
	/// </summary>
	public class DebugHandler: SocketHandlerBase {
		public DebugHandler(SocketRequestHandler handler) :
		base(handler, "Debug") {
			server.dailyCallbacks.Register(Update);
		}

		/// <summary>
		/// Called each frame to send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(DailyCallbackParam param) {
			SendAll();
		}

		/// <summary>
		/// Send all debug info to client.
		/// </summary>
		protected void SendAll() {
			SendJson(new Dictionary<string, int> {
				{"numActiveHandlers", this.server.NumActiveHandlers},
			});
		}
	}
}