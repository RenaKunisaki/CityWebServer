using CityWebServer.RequestHandlers;
using CityWebServer.Callbacks;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Pushes updated terrain info to client.
	/// </summary>
	public class TerrainHandler: SocketHandlerBase {
		public TerrainHandler(SocketRequestHandler handler) :
		base(handler, "Terrain") {
			server.terrainCallbacks.Register(Update);
		}

		/// <summary>
		/// Callback when terrain is changed.
		/// </summary>
		/// <param name="param">Area that was changed.</param>
		protected void Update(TerrainCallbackParam param) {
			SendJson(param, "TerrainUpdated");
		}
	}
}