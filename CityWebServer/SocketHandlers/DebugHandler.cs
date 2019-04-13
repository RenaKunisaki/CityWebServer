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
		protected float totalTimeDelta, updateInterval;

		public DebugHandler(SocketRequestHandler handler) :
		base(handler, "Debug") {
			totalTimeDelta = 0;
			updateInterval = 0; //seconds (0=disable)
			server.frameCallbacks.Register(Update);
		}

		/// <summary>
		/// Called each frame to send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(FrameCallbackParam param) {
			if(updateInterval <= 0) return; //reporting disabled
			totalTimeDelta += param.realTimeDelta;
			if(totalTimeDelta >= updateInterval) {
				SendAll();
				totalTimeDelta = 0;
			}
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