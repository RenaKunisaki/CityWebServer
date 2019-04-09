using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about object instances and other runtime stats.
	/// </summary>
	public class InstancesHandler: SocketHandlerBase {
		public InstancesHandler(SocketRequestHandler handler) :
		base(handler, "Instances") {
			SendAll();
		}

		protected void SendAll() {
			Dictionary<String, Dictionary<String, float>> instances =
			new Dictionary<String, Dictionary<String, float>> {
				["AudioManager"] = GetAudio(),
				["BuildingManager"] = GetBuildings(),
				["CitizenManager"] = GetCitizens(),
				["DisasterManager"] = GetDisasters(),
				["DistrictManager"] = GetDistricts(),
				["EconomyManager"] = GetEconomy(),
				["ElectricityManager"] = GetElectricity(),
				["EventManager"] = GetEvents(),
				["GameAreaManager"] = GetGameAreas(),

				//["ImmaterialResourceManager"] = new Dictionary<String, float> {
				//nothing here...
				//},

				//["MessageManager"] = new Dictionary<String, float> {
				//nothing here...
				//},

				//["NaturalResourceManager"] = new Dictionary<String, float> {
				//some methods we could call with a rect covering entire map?
				//},

				["NetManager"] = GetNetworks(),
				["PathManager"] = GetPaths(),
				["PropManager"] = GetProps(),
				["RenderManager"] = GetRender(),
				["SimulationManager"] = GetSimulation(),
				["TerrainManager"] = GetTerrain(),
				["TransferManager"] = GetTransfers(),
				["TransportManager"] = GetTransport(),
				["TreeManager"] = GetTrees(),
				["VehicleManager"] = GetVehicles(),
				["WeatherManager"] = GetWeather(),

				//["WaterManager"] = new Dictionary<String, float> {
				//Some methods that require a position
				//},

				["ZoneManager"] = GetZones(),
			};
			SendJson(instances);
		}

		public Dictionary<String, float> GetAudio() {
			var manager = Singleton<AudioManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_radioChannelCount"] = manager.m_radioChannelCount,
				["m_radioChannelInfoCount"] = manager.m_radioChannelInfoCount,
				["m_radioContentCount"] = manager.m_radioContentCount,
				["m_radioContentInfoCount"] = manager.m_radioContentInfoCount,
			};
		}

		public Dictionary<String, float> GetBuildings() {
			var manager = Singleton<BuildingManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_buildingCount"] = manager.m_buildingCount,
				["m_infoCount"] = manager.m_infoCount,
				["m_abandonmentDisabled"] = manager.m_abandonmentDisabled ? 1 : 0,
				["m_firesDisabled"] = manager.m_firesDisabled ? 1 : 0,
			};
		}

		public Dictionary<String, float> GetCitizens() {
			var manager = Singleton<CitizenManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_citizenCount"] = manager.m_citizenCount,
				["m_finalOldestOriginalResident"] = manager.m_finalOldestOriginalResident,
				["m_fullyEducatedOriginalResidents"] = manager.m_fullyEducatedOriginalResidents,
				["m_infoCount"] = manager.m_infoCount,
				["m_instanceCount"] = manager.m_instanceCount,
				["m_unitCount"] = manager.m_unitCount,
			};
		}

		public Dictionary<String, float> GetDisasters() {
			var manager = Singleton<DisasterManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_detectedDisasterCount"] = manager.m_detectedDisasterCount,
				["m_disableAutomaticFollow"] = manager.m_disableAutomaticFollow ? 1 : 0,
				["m_disableCameraShake"] = manager.m_disableCameraShake ? 1 : 0,
				["m_disasterCount"] = manager.m_disasterCount,
				["m_infoCount"] = manager.m_infoCount,
				["m_randomDisasterCooldown"] = manager.m_randomDisasterCooldown,
			};
		}

		public Dictionary<String, float> GetDistricts() {
			var manager = Singleton<DistrictManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_districtCount"] = manager.m_districtCount,
				["m_parkCount"] = manager.m_parkCount,
			};
		}

		public Dictionary<String, float> GetEconomy() {
			var manager = Singleton<EconomyManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["InternalCashAmount"] = manager.InternalCashAmount,
				["LastCashAmount"] = manager.LastCashAmount,
				["LastCashDelta"] = manager.LastCashDelta,
				["StartMoney"] = manager.StartMoney,
			};
		}

		public Dictionary<String, float> GetElectricity() {
			var manager = Singleton<ElectricityManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["ElectricityMapVisible"] = manager.ElectricityMapVisible ? 1 : 0,
			};
		}

		public Dictionary<String, float> GetEvents() {
			var manager = Singleton<EventManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_bandPopularityBonus"] = manager.m_bandPopularityBonus,
				["m_eventCount"] = manager.m_eventCount,
				["m_finalBandPopularityBonus"] = manager.m_finalBandPopularityBonus,
				["m_infoCount"] = manager.m_infoCount,
			};
		}

		public Dictionary<String, float> GetGameAreas() {
			var manager = Singleton<GameAreaManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["MaxAreaCount"] = manager.MaxAreaCount,
				["m_areaCount"] = manager.m_areaCount,
				["m_maxAreaCount"] = manager.m_maxAreaCount,
			};
		}

		public Dictionary<String, float> GetNetworks() {
			var manager = Singleton<NetManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_currentMaxWetness"] = manager.m_currentMaxWetness,
				["m_infoCount"] = manager.m_infoCount,
				["m_laneCount"] = manager.m_laneCount,
				["m_lastMaxWetness"] = manager.m_lastMaxWetness,
				["m_nodeCount"] = manager.m_nodeCount,
				["m_segmentCount"] = manager.m_segmentCount,
				["m_treatWetAsSnow"] = manager.m_treatWetAsSnow ? 1 : 0,
				["m_wetnessChanged"] = manager.m_wetnessChanged,
			};
		}

		public Dictionary<String, float> GetPaths() {
			var manager = Singleton<PathManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_pathUnitCount"] = manager.m_pathUnitCount,
				["m_renderPathGizmo"] = manager.m_renderPathGizmo,
			};
		}

		public Dictionary<String, float> GetProps() {
			var manager = Singleton<PropManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_infoCount"] = manager.m_infoCount,
				["m_propCount"] = manager.m_propCount,
			};
		}

		public Dictionary<String, float> GetRender() {
			var manager = Singleton<RenderManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["CameraHeight"] = manager.CameraHeight,
			};
		}

		public Dictionary<String, float> GetSimulation() {
			var manager = Singleton<SimulationManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["FinalSimulationSpeed"] = manager.FinalSimulationSpeed,
				["FinalSimulationSpeed"] = manager.ForcedSimulationPaused ? 1 : 0,
				["m_currentDayTimeHour"] = manager.m_currentDayTimeHour,
				["m_currentBuildIndex"] = manager.m_currentBuildIndex,
				["m_currentFrameIndex"] = manager.m_currentFrameIndex,
				["m_currentTickIndex"] = manager.m_currentTickIndex,
				["m_dayTimeFrame"] = manager.m_dayTimeFrame,
				["m_dayTimeOffsetFrames"] = manager.m_dayTimeOffsetFrames,
				["m_enableDayNight"] = manager.m_enableDayNight ? 1 : 0,
				["m_isNightTime"] = manager.m_isNightTime ? 1 : 0,
				["m_referenceFrameIndex"] = manager.m_referenceFrameIndex,
				["m_realTimer"] = manager.m_realTimer,
				["m_referenceTimer"] = manager.m_referenceTimer,
				["m_simulationTimeDelta"] = manager.m_simulationTimeDelta,
				["m_simulationTimer"] = manager.m_simulationTimer,
				["m_simulationTimer2"] = manager.m_simulationTimer2,
				["m_simulationTimeSpeed"] = manager.m_simulationTimeSpeed,
				["m_timeOffsetTicks"] = manager.m_timeOffsetTicks,
				["SelectedSimulationSpeed"] = manager.SelectedSimulationSpeed,
				["SimulationPaused"] = manager.SimulationPaused ? 1 : 0,
				["Terminated"] = manager.Terminated ? 1 : 0,
			};
		}

		public Dictionary<String, float> GetTerrain() {
			var manager = Singleton<TerrainManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["DirtBuffer"] = manager.DirtBuffer,
				["m_detailPatchCount"] = manager.m_detailPatchCount,
				["m_modifyingHeights"] = manager.m_modifyingHeights ? 1 : 0,
				["m_modifyingLevel"] = manager.m_modifyingLevel,
				["m_modifyingSurface"] = manager.m_modifyingSurface ? 1 : 0,
				["m_modifyingZones"] = manager.m_modifyingZones ? 1 : 0,
				["RawDirtBuffer"] = manager.RawDirtBuffer,
			};
		}

		public Dictionary<String, float> GetTransfers() {
			var manager = Singleton<TransferManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_unitCount"] = manager.m_unitCount,
			};
		}

		public Dictionary<String, float> GetTransport() {
			var manager = Singleton<TransportManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_infoCount"] = manager.m_infoCount,
				["m_lineCount"] = manager.m_lineCount,
			};
		}

		public Dictionary<String, float> GetTrees() {
			var manager = Singleton<TreeManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_infoCount"] = manager.m_infoCount,
				["m_treeCount"] = manager.m_treeCount,
			};
		}

		public Dictionary<String, float> GetVehicles() {
			var manager = Singleton<VehicleManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_infoCount"] = manager.m_infoCount,
				["m_lastTrafficFlow"] = manager.m_lastTrafficFlow,
				["m_maxTrafficFlow"] = manager.m_maxTrafficFlow,
				["m_parkedCount"] = manager.m_parkedCount,
				["m_totalTrafficFlow"] = manager.m_totalTrafficFlow,
				["m_vehicleCount"] = manager.m_vehicleCount,
			};
		}

		public Dictionary<String, float> GetWeather() {
			var manager = Singleton<WeatherManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_currentCloud"] = manager.m_currentCloud,
				["m_currentFog"] = manager.m_currentFog,
				["m_currentNorthernLights"] = manager.m_currentNorthernLights,
				["m_currentRain"] = manager.m_currentRain,
				["m_currentRainbow"] = manager.m_currentRainbow,
				["m_currentTemperature"] = manager.m_currentTemperature,
				["m_directionSpeed"] = manager.m_directionSpeed,
				["m_enableWeather"] = manager.m_enableWeather ? 1 : 0,
				["m_flashingReduction"] = manager.m_flashingReduction,
				["m_forceWeatherOn"] = manager.m_forceWeatherOn,
				["m_groundWetness"] = manager.m_groundWetness,
				["m_lastLightningIntensity"] = manager.m_lastLightningIntensity,
				["m_targetCloud"] = manager.m_targetCloud,
				["m_targetDirection"] = manager.m_targetDirection,
				["m_targetFog"] = manager.m_targetFog,
				["m_targetNorthernLights"] = manager.m_targetNorthernLights,
				["m_targetRain"] = manager.m_targetRain,
				["m_targetRainbow"] = manager.m_targetRainbow,
				["m_targetTemperature"] = manager.m_targetTemperature,
				["m_temperatureSpeed"] = manager.m_temperatureSpeed,
				["m_windDirection"] = manager.m_windDirection,
			};
		}

		public Dictionary<String, float> GetZones() {
			var manager = Singleton<ZoneManager>.instance;
			if(manager == null) return null;
			return new Dictionary<String, float> {
				["m_actualCommercialDemand"] = manager.m_actualCommercialDemand,
				["m_actualResidentialDemand"] = manager.m_actualResidentialDemand,
				["m_actualWorkplaceDemand"] = manager.m_actualWorkplaceDemand,
				["m_blockCount"] = manager.m_blockCount,
				["m_commercialDemand"] = manager.m_commercialDemand,
				["m_lastBuildIndex"] = manager.m_lastBuildIndex,
				["m_residentialDemand"] = manager.m_residentialDemand,
				["m_workplaceDemand"] = manager.m_workplaceDemand,
			};
		}
	}
}
