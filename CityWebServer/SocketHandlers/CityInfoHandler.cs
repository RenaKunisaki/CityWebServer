﻿using System;
using System.Collections.Generic;
using System.Linq;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using JetBrains.Annotations;

namespace CityWebServer.SocketHandlers {
	[UsedImplicitly]
	///Pushes updated city info to client.
	public class CityInfoHandler: SocketHandlerBase {
		protected float totalTimeDelta;

		public CityInfoHandler(SocketRequestHandler handler) :
		base(handler, "CityInfo") {
			totalTimeDelta = 0;
			(handler.Server as WebServer).RegisterFrameCallback(Update);
			SendInitialInfo();
		}

		/// <summary>
		/// Send initial city info to new client.
		/// </summary>
		protected void SendInitialInfo() {
			var simulationManager = Singleton<SimulationManager>.instance;
			SendJson(new InitialCityInfo {
				Name = simulationManager.m_metaData.m_CityName,
				mapName = simulationManager.m_metaData.m_MapName,
				environment = simulationManager.m_metaData.m_environment,
				//GlobalDistrict = globalDistrictInfo,
				//Districts = districtInfoList.ToArray(),
				isTileUnlocked = GetUnlockedTiles(),
			});
			SendNewInfo(); //also send that info that isn't included above
		}

		/// <summary>
		/// Send updated info to existing client.
		/// </summary>
		protected void SendNewInfo() {
			var simulationManager = Singleton<SimulationManager>.instance;
			SendJson(new VolatileCityInfo {
				Time = simulationManager.m_currentGameTime,
				isNight = simulationManager.m_isNightTime,
				simSpeed = simulationManager.SelectedSimulationSpeed,
				isPaused = simulationManager.SimulationPaused,
			}, "Frame");
		}

		/// <summary>
		/// Called each frame to send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(FrameCallbackParam param) {
			totalTimeDelta += param.realTimeDelta;
			if(totalTimeDelta >= 1) { //only update once per second
				SendNewInfo();
				totalTimeDelta = 0;
			}
		}

		/// <summary>
		/// Get list of unlock flags for each tile.
		/// </summary>
		/// <returns>A Boolean for each tile, where true=unlocked.</returns>
		public Boolean[] GetUnlockedTiles() {
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
			return isUnlocked;
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