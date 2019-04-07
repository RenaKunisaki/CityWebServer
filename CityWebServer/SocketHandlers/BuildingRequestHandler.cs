using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using ColossalFramework;

namespace CityWebServer.RequestHandlers {
	public class BuildingRequestHandler: RequestHandlerBase {
		/** Handles `/Building`.
		 *  Returns information about buildings.
		 */
		public BuildingRequestHandler(IWebServer server)
			: base(server, new Guid("03897cb0-d53f-4189-a613-e7d22705dc2f"), "Building", "Rychard", 100, "/Building") {
		}

		public override void Handle(HttpRequest request) {
			this.request = request;
			var buildingManager = Singleton<BuildingManager>.instance;

			if(request.path.StartsWith("/Building/List")) {
				List<ushort> buildingIDs = new List<ushort>();

				var len = buildingManager.m_buildings.m_buffer.Length;
				for(ushort i = 0; i < len; i++) {
					if(buildingManager.m_buildings.m_buffer[i].m_flags == Building.Flags.None) { continue; }

					buildingIDs.Add(i);
				}

				SendJson(buildingIDs);
			}

			foreach(var building in buildingManager.m_buildings.m_buffer) {
				if(building.m_flags == Building.Flags.None) { continue; }

				// TODO: Something with Buildings.
			}

			SendJson("");
		}
	}
}