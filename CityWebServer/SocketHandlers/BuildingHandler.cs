using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends building info to client.
	/// </summary>
	public class BuildingHandler: SocketHandlerBase {
		public BuildingHandler(SocketRequestHandler handler) :
		base(handler, "Building") {
			//SendAll();
			//This causes a Write Failure and nothing gets sent.
			//Probably it's sending too much at once.
		}

		/// <summary>
		/// Send list of all buildings.
		/// </summary>
		protected void SendAll() {
			Log("Getting building list...");
			var buildingManager = Singleton<BuildingManager>.instance;
			var buildings = new Dictionary<int, CityWebServer.Models.BuildingInfo>();
			for(int i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++) {
				var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[i];
				if(building.m_flags == Building.Flags.None) continue;
				var info = GetBuilding(i);
				if(info != null) buildings[i] = info;
			}
			Log("Sending building list...");
			SendJson(buildings);
			Log("Sent building list.");
		}

		/// <summary>
		/// Send info about one building.
		/// </summary>
		/// <param name="id">Building ID.</param>
		protected void SendBuilding(int id) {
			SendJson(GetBuilding(id));
		}

		/// <summary>
		/// Get info about specified building by ID.
		/// </summary>
		/// <returns>The building info.</returns>
		/// <param name="id">Building ID.</param>
		protected CityWebServer.Models.BuildingInfo GetBuilding(int id) {
			Building building;
			try {
				building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
			}
			catch(NullReferenceException) {
				return null;
			}
			if(building.Info == null) return null;
			return new CityWebServer.Models.BuildingInfo {
				category = building.Info.category,
				title = building.Info.GetUncheckedLocalizedTitle(),
				thumbnail = building.Info.m_Thumbnail,
				classLevel = (int)building.Info.GetClassLevel(),
				maintenanceCost = building.Info.GetMaintenanceCost(),
				service = (int)building.Info.GetService(),
				subService = (int)building.Info.GetSubService(),
				cellLength = building.Info.m_cellLength,
				cellWidth = building.Info.m_cellWidth,
				adults = building.m_adults,
				baseHeight = building.m_baseHeight,
				buildIndex = building.m_buildIndex,
				buildWaterHeight = building.m_buildWaterHeight,
				citizenCount = building.m_citizenCount,
				citizenUnits = building.m_citizenUnits,
				crimeBuffer = building.m_crimeBuffer,
				customBuffer1 = building.m_customBuffer1,
				customBuffer2 = building.m_customBuffer2,
				deathProblemTimer = building.m_deathProblemTimer,
				education1 = building.m_education1,
				education2 = building.m_education2,
				education3 = building.m_education3,
				electricityBuffer = building.m_electricityBuffer,
				electricityProblemTimer = building.m_electricityProblemTimer,
				eventIndex = building.m_eventIndex,
				finalExport = building.m_finalExport,
				finalImport = building.m_finalImport,
				fireHazard = building.m_fireHazard,
				fireIntensity = building.m_fireIntensity,
				//flags = building.m_flags,
				garbageBuffer = building.m_garbageBuffer,
				guestVehicles = building.m_guestVehicles,
				happiness = building.m_happiness,
				health = building.m_health,
				healthProblemTimer = building.m_healthProblemTimer,
				heatingBuffer = building.m_heatingBuffer,
				heatingProblemTimer = building.m_heatingProblemTimer,
				incomingProblemTimer = building.m_incomingProblemTimer,
				infoIndex = building.m_infoIndex,
				length = building.m_length,
				level = building.m_level,
				levelUpProgress = building.m_levelUpProgress,
				mailBuffer = building.m_mailBuffer,
				majorProblemTimer = building.m_majorProblemTimer,
				netNode = building.m_netNode,
				nextGridBuilding = building.m_nextGridBuilding,
				nextGridBuilding2 = building.m_nextGridBuilding2,
				outgoingProblemTimer = building.m_outgoingProblemTimer,
				ownVehicles = building.m_ownVehicles,
				parentBuilding = building.m_parentBuilding,
				//position = building.m_position,
				//problems = building.m_problems,
				productionRate = building.m_productionRate,
				seniors = building.m_seniors,
				serviceProblemTimer = building.m_serviceProblemTimer,
				sewageBuffer = building.m_sewageBuffer,
				sourceCitizens = building.m_sourceCitizens,
				subBuilding = building.m_subBuilding,
				targetCitizens = building.m_targetCitizens,
				taxProblemTimer = building.m_taxProblemTimer,
				teens = building.m_teens,
				waterBuffer = building.m_waterBuffer,
				waterPollution = building.m_waterPollution,
				waterProblemTimer = building.m_waterProblemTimer,
				waterSource = building.m_waterSource,
				width = building.m_width,
				workerProblemTimer = building.m_workerProblemTimer,
				youngs = building.m_youngs,
			};

		}
	}
}