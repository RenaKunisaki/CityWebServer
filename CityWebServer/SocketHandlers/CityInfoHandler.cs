using System;
using System.Collections.Generic;
using System.Linq;
using CityWebServer.Models;
using ColossalFramework;
using JetBrains.Annotations;

namespace CityWebServer.RequestHandlers {
	[UsedImplicitly]
	public class CityInfoHandler: SocketHandlerBase {
		public CityInfoHandler(SocketRequestHandler handler) :
		base(handler, "CityInfo") {
			(handler.Server as WebServer).RegisterFrameCallback(Update);
		}

		protected void Update(object param) {
			Log("CityInfoHandler update");
			var simulationManager = Singleton<SimulationManager>.instance;
			SendJson(simulationManager.m_metaData.m_currentDateTime);
		}

		/* public override void Handle(HttpRequest request) {
			this.request = request;
			if(request.QueryString.HasKey("showList")) {
				return HandleDistrictList();
			}
			HandleDistrict(request);
		} */

		private void HandleDistrictList() {
			var districtIDs = DistrictInfo.GetDistricts().ToArray();
			SendJson(districtIDs);
		}

		private void HandleDistrict(HttpRequest request) {
			var districtIDs = GetDistrictsFromRequest(request);

			DistrictInfo globalDistrictInfo = null;
			List<DistrictInfo> districtInfoList = new List<DistrictInfo>();

			var buildings = GetBuildingBreakdownByDistrict();
			var vehicles = GetVehicleBreakdownByDistrict();

			foreach(var districtID in districtIDs) {
				var districtInfo = DistrictInfo.GetDistrictInfo(districtID);
				if(districtID == 0) {
					districtInfo.TotalBuildingCount = buildings.Sum(obj => obj.Value);
					districtInfo.TotalVehicleCount = vehicles.Sum(obj => obj.Value);
					globalDistrictInfo = districtInfo;
				}
				else {
					districtInfo.TotalBuildingCount = buildings.Where(obj => obj.Key == districtID).Sum(obj => obj.Value);
					districtInfo.TotalVehicleCount = vehicles.Where(obj => obj.Key == districtID).Sum(obj => obj.Value);
					districtInfoList.Add(districtInfo);
				}
			}

			var simulationManager = Singleton<SimulationManager>.instance;
			var gameAreaManager = Singleton<GameAreaManager>.instance;
			Boolean[] isUnlocked = new Boolean[gameAreaManager.MaxAreaCount];
			int x = 0, y = 0;
			for(int i = 0; i < gameAreaManager.MaxAreaCount; i++) {
				isUnlocked[i] = gameAreaManager.IsUnlocked(x, y);
				x++;
				if(x == GameAreaManager.TOTAL_AREA_RESOLUTION) {
					x = 0;
					y++;
				}
				//XXX how will this work with 81 tile mod?
			}

			var cityInfo = new CityInfo {
				Name = simulationManager.m_metaData.m_CityName,
				mapName = simulationManager.m_metaData.m_MapName,
				environment = simulationManager.m_metaData.m_environment,
				Time = simulationManager.m_currentGameTime,
				GlobalDistrict = globalDistrictInfo,
				Districts = districtInfoList.ToArray(),
				isNight = simulationManager.m_isNightTime,
				simSpeed = simulationManager.SelectedSimulationSpeed,
				isPaused = simulationManager.SimulationPaused,
				isTileUnlocked = isUnlocked,
			};

			SendJson(cityInfo);
		}

		private Dictionary<int, int> GetBuildingBreakdownByDistrict() {
			var districtManager = Singleton<DistrictManager>.instance;

			Dictionary<int, int> districtBuildings = new Dictionary<int, int>();
			BuildingManager instance = Singleton<BuildingManager>.instance;
			foreach(Building building in instance.m_buildings.m_buffer) {
				if(building.m_flags == Building.Flags.None) { continue; }
				var districtID = (int)districtManager.GetDistrict(building.m_position);
				if(districtBuildings.ContainsKey(districtID)) {
					districtBuildings[districtID]++;
				}
				else {
					districtBuildings.Add(districtID, 1);
				}
			}
			return districtBuildings;
		}

		private Dictionary<int, int> GetVehicleBreakdownByDistrict() {
			var districtManager = Singleton<DistrictManager>.instance;

			Dictionary<int, int> districtVehicles = new Dictionary<int, int>();
			VehicleManager vehicleManager = Singleton<VehicleManager>.instance;
			foreach(Vehicle vehicle in vehicleManager.m_vehicles.m_buffer) {
				if(vehicle.m_flags != 0) {
					var districtID = (int)districtManager.GetDistrict(vehicle.GetLastFramePosition());
					if(districtVehicles.ContainsKey(districtID)) {
						districtVehicles[districtID]++;
					}
					else {
						districtVehicles.Add(districtID, 1);
					}
				}
			}
			return districtVehicles;
		}

		private IEnumerable<int> GetDistrictsFromRequest(HttpRequest request) {
			IEnumerable<int> districtIDs;
			/* if(request.QueryString.HasKey("districtID")) {
				List<int> districtIDList = new List<int>();
				var districtID = request.QueryString.GetInteger("districtID");
				if(districtID.HasValue) {
					districtIDList.Add(districtID.Value);
				}
				districtIDs = districtIDList;
			}
			else { */
			districtIDs = DistrictInfo.GetDistricts();
			//}
			return districtIDs;
		}
	}
}