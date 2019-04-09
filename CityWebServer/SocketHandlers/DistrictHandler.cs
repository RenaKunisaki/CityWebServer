using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Pushes district info to client.
	/// </summary>
	public class DistrictHandler: SocketHandlerBase {
		protected float totalTimeDelta;

		public DistrictHandler(SocketRequestHandler handler) :
		base(handler, "District") {
			totalTimeDelta = 0;
			server.frameCallbacks.Register(Update);
			handler.RegisterMessageHandler("District", OnDistrictMessage);
			SendDistrict(0);
		}

		/// <summary>
		/// Called each frame to send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(FrameCallbackParam param) {
			totalTimeDelta += param.realTimeDelta;
			if(totalTimeDelta >= 1) { //only update once per second
				SendDistrict(0); //0 = whole city
				totalTimeDelta = 0;
			}
		}

		public DistrictInfo GetDistrict(int districtID) {
			var districtManager = Singleton<DistrictManager>.instance;
			var district = districtManager.m_districts.m_buffer[districtID];

			//District 0 is the entire city.
			//It does have an auto-generated name, but this name is
			//never shown in-game. (why not just use that to store
			//the city name?)
			string name = "";
			if(districtID != 0) name = districtManager.GetDistrictName(districtID);

			return new DistrictInfo {
				ID = (uint)districtID,
				Name = name,
				Population = district.m_populationData.m_finalCount,
				PopDelta = district.m_populationData.GetWeeklyDelta(),
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
		/// Handle "District" message from client.
		/// </summary>
		/// <param name="_param">Parameter.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: district ID => get info about specified district
		/// <c>list</c>: (anything) => get list of valid IDs
		/// </remarks>
		public void OnDistrictMessage(SocketMessageHandlerParam _param) {
			var param = _param.param as Dictionary<string, object>;
			var key = param.Keys.First();
			switch(key) {
				case null:
					SendErrorResponse(HttpStatusCode.BadRequest);
					break;
				case "get":
					int? id = param["get"] as int?;
					if(id == null) {
						SendErrorResponse("Invalid line ID");
						return;
					}
					SendJson(GetDistrict((int)id), "District");
					break;
				case "list":
					SendDistrictList();
					break;
				default:
					SendErrorResponse($"District has no method '{key}'");
					break;
			}
		}

		/// <summary>
		/// Send a list of valid district IDs.
		/// </summary>
		protected void SendDistrictList() {
			var districtManager = Singleton<DistrictManager>.instance;
			if(districtManager == null) {
				SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return;
			}

			List<int> ids = new List<int>();
			District[] districts;
			try {
				districts = districtManager.m_districts.m_buffer;
			}
			catch(Exception ex) {
				Log($"Error getting districts buffer: {ex}");
				throw;
			}

			for(int i = 0; i < districts.Length; i++) {
				var district = districts[i];
				if(district.m_flags == District.Flags.None) continue;
				ids.Add(i);
			}

			SendJson(ids, "DistrictIDs");
		}

		/// <summary>
		/// Send district info to client.
		/// </summary>
		/// <param name="id">District ID.</param>
		protected void SendDistrict(int id) {
			SendJson(GetDistrict(id));
		}
	}
}