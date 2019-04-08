using System;
using System.Collections.Generic;
using System.Linq;
using CityWebServer.Callbacks;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using JetBrains.Annotations;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Pushes updated city info to client.
	/// </summary>
	public class CityInfoHandler: SocketHandlerBase {
		protected float totalTimeDelta;
		protected int demandR, demandC, demandW;

		public CityInfoHandler(SocketRequestHandler handler) :
		base(handler, "CityInfo") {
			totalTimeDelta = 0;
			server.frameCallbacks.Register(Update);
			server.unlockAreaCallbacks.Register(OnAreaUnlocked);
			server.updateDemandCallbacks.Register(OnUpdateDemand);
			SendInitialInfo();
		}

		/// <summary>
		/// Send initial city info to new client.
		/// </summary>
		protected void SendInitialInfo() {
			var simulationManager = Singleton<SimulationManager>.instance;
			var meta = simulationManager.m_metaData;
			SendJson(new InitialCityInfo {
				Name = meta.m_CityName,
				mapName = meta.m_MapName,
				environment = meta.m_environment,
				invertTraffic = meta.m_invertTraffic == SimulationMetaData.MetaBool.True,
				startingDateTime = meta.m_startingDateTime,
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
			var citizenManager = Singleton<CitizenManager>.instance;
			var districtManager = Singleton<DistrictManager>.instance;
			var district = districtManager.m_districts.m_buffer[0];
			var vehicleManager = Singleton<VehicleManager>.instance;

			SendJson(new VolatileCityInfo {
				Time = simulationManager.m_currentGameTime,
				isNight = simulationManager.m_isNightTime,
				simSpeed = simulationManager.SelectedSimulationSpeed,
				isPaused = simulationManager.SimulationPaused,
				demandR = demandR,
				demandC = demandC,
				demandW = demandW,
				citizenCount = citizenManager.m_citizenCount,
				trafficFlow = vehicleManager.m_lastTrafficFlow,
				vehicleCount = vehicleManager.m_vehicleCount,
				parkedCount = vehicleManager.m_parkedCount,
				cityInfo = GetDistrict(0),
			}, "Tick");
		}


		public DistrictInfo GetDistrict(int districtID) {
			var districtManager = Singleton<DistrictManager>.instance;
			var district = districtManager.m_districts.m_buffer[districtID];
			string name = "City";
			if(districtID != 0) name = districtManager.GetDistrictName(districtID);

			return new DistrictInfo {
				name = name,
				population = district.m_populationData.m_finalCount,
				popDelta = district.m_populationData.GetWeeklyDelta(),
				CremateCapacity = district.GetCremateCapacity(),
				CriminalAmount = district.GetCriminalAmount(),
				CriminalCapacity = district.GetCriminalCapacity(),
				DeadAmount = district.GetDeadAmount(),
				DeadCapacity = district.GetDeadCapacity(),
				DeadCount = district.GetDeadCount(),
				Education1Capacity = district.GetEducation1Capacity(),
				Education1Need = district.GetEducation1Need(),
				Education1Rate = district.GetEducation1Rate(),
				Education2Capacity = district.GetEducation2Capacity(),
				Education2Need = district.GetEducation2Need(),
				Education2Rate = district.GetEducation2Rate(),
				Education3Capacity = district.GetEducation3Capacity(),
				Education3Need = district.GetEducation3Need(),
				Education3Rate = district.GetEducation3Rate(),
				ElectricityCapacity = district.GetElectricityCapacity(),
				ElectricityConsumption = district.GetElectricityConsumption(),
				ExportAmount = district.GetExportAmount(),
				ExtraCriminals = district.GetExtraCriminals(),
				GarbageAccumulation = district.GetGarbageAccumulation(),
				GarbageAmount = district.GetGarbageAmount(),
				GarbageCapacity = district.GetGarbageCapacity(),
				GarbagePiles = district.GetGarbagePiles(),
				GroundPollution = district.GetGroundPollution(),
				HealCapacity = district.GetHealCapacity(),
				HeatingCapacity = district.GetHeatingCapacity(),
				HeatingConsumption = district.GetHeatingConsumption(),
				ImportAmount = district.GetImportAmount(),
				IncinerationCapacity = district.GetIncinerationCapacity(),
				IncomeAccumulation = district.GetIncomeAccumulation(),
				LandValue = district.GetLandValue(),
				SewageAccumulation = district.GetSewageAccumulation(),
				SewageCapacity = district.GetSewageCapacity(),
				ShelterCitizenCapacity = district.GetShelterCitizenCapacity(),
				ShelterCitizenNumber = district.GetShelterCitizenNumber(),
				SickCount = district.GetSickCount(),
				Unemployment = district.GetUnemployment(),
				WaterCapacity = district.GetWaterCapacity(),
				WaterConsumption = district.GetWaterConsumption(),
				WaterPollution = district.GetWaterPollution(),
				WaterStorageAmount = district.GetWaterStorageAmount(),
				WaterStorageCapacity = district.GetWaterStorageCapacity(),
				WorkerCount = district.GetWorkerCount(),
				WorkplaceCount = district.GetWorkplaceCount(),
			};
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
		/// Called when a new area is unlocked.
		/// </summary>
		/// <param name="param">Parameter.</param>
		protected void OnAreaUnlocked(UnlockAreaCallbackParam param) {
			SendJson(param, "UnlockArea");
		}

		protected void OnUpdateDemand(UpdateDemandParam param) {
			switch(param.which) {
				case 'R': demandR = param.demand; break;
				case 'C': demandC = param.demand; break;
				case 'W': demandW = param.demand; break;
				default: Log($"Unknown demand type '{param.which}'"); break;
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

		/* private void HandleDistrictList() {
			var districtIDs = DistrictInfo.GetDistricts().ToArray();
			SendJson(districtIDs);
		} */

		/* private void HandleDistrict(HttpRequest request) {
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




		} */

		/* private Dictionary<int, int> GetBuildingBreakdownByDistrict() {
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
		 */
		/* private IEnumerable<int> GetDistrictsFromRequest(HttpRequest request) {
			IEnumerable<int> districtIDs;
			if(request.QueryString.HasKey("districtID")) {
				List<int> districtIDList = new List<int>();
				var districtID = request.QueryString.GetInteger("districtID");
				if(districtID.HasValue) {
					districtIDList.Add(districtID.Value);
				}
				districtIDs = districtIDList;
			}
			else {
				districtIDs = DistrictInfo.GetDistricts();
			}
			return districtIDs;
		} */
	}
}