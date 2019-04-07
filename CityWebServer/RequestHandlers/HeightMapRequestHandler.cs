using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using ColossalFramework;

namespace CityWebServer.RequestHandlers {
	public class HeightMapRequestHandler: RequestHandlerBase {
		/** Handles `/HeightMap`.
		 *  Returns the terrain height map.
		 */
		public HeightMapRequestHandler()
			: base(new Guid("53534fbb-a56d-4f3e-99da-cdbdea1c0c17"),
				"HeightMap", "Rena", 100, "/HeightMap") {
		}

		public HeightMapRequestHandler(WebServer server, HttpRequest request, String name)
		: base(server, request, name) { }

		public override void Handle() {
			byte[] map = Singleton<TerrainManager>.instance.GetHeightmap();
			SendBinary(map);
		}
	}
}
