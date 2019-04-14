using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using CityWebServer.Callbacks;
using System.Linq;
using UnityEngine;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends building info to client.
	/// </summary>
	public class BuildingHandler: SocketHandlerBase {
		//XXX this is copied from NotificationHandler
		public static Dictionary<string, ulong> ProblemFlags =
		new Dictionary<string, ulong>() {
			["Crime"] = (ulong)Notification.Problem.Crime,
			["Death"] = (ulong)Notification.Problem.Death,
			["DepotNotConnected"] = (ulong)Notification.Problem.DepotNotConnected,
			["DirtyWater"] = (ulong)Notification.Problem.DirtyWater,
			["Electricity"] = (ulong)Notification.Problem.Electricity,
			["ElectricityNotConnected"] = (ulong)Notification.Problem.ElectricityNotConnected,
			["Emptying"] = (ulong)Notification.Problem.Emptying,
			["EmptyingFinished"] = (ulong)Notification.Problem.EmptyingFinished,
			["Evacuating"] = (ulong)Notification.Problem.Evacuating,
			["FatalProblem"] = (ulong)Notification.Problem.FatalProblem,
			["Fire"] = (ulong)Notification.Problem.Fire,
			["Flood"] = (ulong)Notification.Problem.Flood,
			["Garbage"] = (ulong)Notification.Problem.Garbage,
			["Heating"] = (ulong)Notification.Problem.Heating,
			["HeatingNotConnected"] = (ulong)Notification.Problem.HeatingNotConnected,
			["LandfillFull"] = (ulong)Notification.Problem.LandfillFull,
			["LandValueLow"] = (ulong)Notification.Problem.LandValueLow,
			["LineNotConnected"] = (ulong)Notification.Problem.LineNotConnected,
			["MajorProblem"] = (ulong)Notification.Problem.MajorProblem,
			["NoCustomers"] = (ulong)Notification.Problem.NoCustomers,
			["NoEducatedWorkers"] = (ulong)Notification.Problem.NoEducatedWorkers,
			["NoFood"] = (ulong)Notification.Problem.NoFood,
			["NoFuel"] = (ulong)Notification.Problem.NoFuel,
			["NoGoods"] = (ulong)Notification.Problem.NoGoods,
			["NoInputProducts"] = (ulong)Notification.Problem.NoInputProducts,
			["Noise"] = (ulong)Notification.Problem.Noise,
			["NoMainGate"] = (ulong)Notification.Problem.NoMainGate,
			["NoNaturalResources"] = (ulong)Notification.Problem.NoNaturalResources,
			["NoPark"] = (ulong)Notification.Problem.NoPark,
			["NoPlaceforGoods"] = (ulong)Notification.Problem.NoPlaceforGoods,
			["NoResources"] = (ulong)Notification.Problem.NoResources,
			["NotInIndustryArea"] = (ulong)Notification.Problem.NotInIndustryArea,
			["NoWorkers"] = (ulong)Notification.Problem.NoWorkers,
			["PathNotConnected"] = (ulong)Notification.Problem.PathNotConnected,
			["Pollution"] = (ulong)Notification.Problem.Pollution,
			["ResourceNotSelected"] = (ulong)Notification.Problem.ResourceNotSelected,
			["RoadNotConnected"] = (ulong)Notification.Problem.RoadNotConnected,
			["Sewage"] = (ulong)Notification.Problem.Sewage,
			["Snow"] = (ulong)Notification.Problem.Snow,
			["StructureDamaged"] = (ulong)Notification.Problem.StructureDamaged,
			["StructureVisited"] = (ulong)Notification.Problem.StructureVisited,
			["StructureVisitedService"] = (ulong)Notification.Problem.StructureVisitedService,
			["TaxesTooHigh"] = (ulong)Notification.Problem.TaxesTooHigh,
			["TooFewServices"] = (ulong)Notification.Problem.TooFewServices,
			["TooLong"] = (ulong)Notification.Problem.TooLong,
			["TrackNotConnected"] = (ulong)Notification.Problem.TrackNotConnected,
			["TurnedOff"] = (ulong)Notification.Problem.TurnedOff,
			["Water"] = (ulong)Notification.Problem.Water,
			["WaterNotConnected"] = (ulong)Notification.Problem.WaterNotConnected,
			["WrongAreaType"] = (ulong)Notification.Problem.WrongAreaType,
		};

		protected class ClientMessage {
			public ClientMessage(SocketMessageHandlerParam _param) {
				this.param = _param.param;
			}

			public string GetString(string key, bool allowNull = false) {
				var p = this.param as Dictionary<string, object>;
				string s = p[key] as string;
				if(s == null && !allowNull) {
					throw new ArgumentException($"invalid value for {key}");
				}
				return s;
			}

			public int GetInt(string key) {
				var p = this.param as Dictionary<string, object>;
				int? val = p[key] as int?;
				if(val == null) {
					throw new ArgumentException($"invalid value for {key}");
				}
				return (int)val;
			}

			public float GetFloat(string key) {
				var p = this.param as Dictionary<string, object>;
				float? val = p[key] as float?;
				if(val == null) {
					throw new ArgumentException($"invalid value for {key}");
				}
				return (float)val;
			}

			protected object param;
		}

		public BuildingHandler(SocketRequestHandler handler) :
		base(handler, "Building") {
			//SendAll();
			handler.RegisterMessageHandler("Building", OnClientMessage);
		}

		/// <summary>
		/// Handle "Building" message from client.
		/// </summary>
		/// <param name="_param">Parameter.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: building ID => get info about specified building
		/// <c>list</c>: (anything) => get list of valid IDs
		/// </remarks>
		public void OnClientMessage(SocketMessageHandlerParam _param) {
			try {
				ClientMessage msg = new ClientMessage(_param);
				string action = msg.GetString("action");
				switch(action) {
					case "get": {
							SendBuilding(msg.GetInt("id"));
							break;
						}
					case "getByProblem": {
							//This could be just a ulong parameter but nope,
							//apparently you can't use ulong in json for reasons
							string flags = msg.GetString("problem");
							if(flags == null || !ProblemFlags.ContainsKey(flags)) {
								throw new ArgumentException("Invalid problem name");
							}
							SendProblems(ProblemFlags[flags]);
							break;
						}
					case "list":
						SendList();
						break;
					case "destroy": {
							DestroyBuilding(msg.GetInt("id"));
							break;
						}
					default:
						throw new ArgumentException($"Invalid method {action}");
				}
			}
			catch(ArgumentException ex) {
				SendErrorResponse(ex.Message);
				return;
			}
		}

		/// <summary>
		/// Send list of all buildings.
		/// </summary>
		/// <remarks>This causes a Write Failure and nothing gets sent.
		/// Probably it's sending too much at once.</remarks>
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
		/// Send list of valid building IDs.
		/// </summary>
		protected void SendList() {
			var buildingManager = Singleton<BuildingManager>.instance;
			var buildings = new List<int>();
			for(int i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++) {
				var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[i];
				if(building.m_flags == Building.Flags.None) continue;
				buildings.Add(i);
			}
			SendJson(buildings, "BuildingIDs");
		}

		/// <summary>
		/// Send info about one building.
		/// </summary>
		/// <param name="id">Building ID.</param>
		protected void SendBuilding(int id) {
			SendJson(GetBuilding(id));
		}

		protected void DestroyBuilding(int id) {
			//Much copied from https://github.com/elboletaire/cities-skylines-destroy-everything/blob/master/Source/Destroyer.cs
			var manager = BuildingManager.instance;
			var target = manager.m_buildings.m_buffer[id];
			if(target.m_flags == Building.Flags.None) {
				SendErrorResponse($"Building doesn't exist: {id}");
				return;
			}
			var info = target.Info;

			//Give refund if possible.
			int amount = target.Info.m_buildingAI.GetRefundAmount(
				(ushort)id, ref target);
			Log($"Destroying building {id} gives refund of {amount}");
			if(amount != 0) {
				EconomyManager.instance.AddResource(
					EconomyManager.Resource.RefundAmount, amount, info.m_class);
			}

			//Get info before destroying
			Vector3 position = target.m_position;
			float angle = target.m_angle;
			int length = target.m_length;
			var lod = info.m_lodMeshData;

			//Delete the building
			Log($"Destroy building: {id}");
			manager.ReleaseBuilding((ushort)id);
			Log($"Building {id} deleted.");

			//Trigger the bulldoze effect
			EffectInfo effect = manager.m_properties.m_bulldozeEffect;
			if(effect != null) {
				Log("Triggering bulldoze effect...");
				var nullAudioGroup = new AudioGroup(0,
					new SavedFloat("Bulldoze",
						Settings.gameSettingsFile, 0, false));
				var instance = new InstanceID();
				var spawnArea = new EffectInfo.SpawnArea(
					Matrix4x4.TRS(
						Building.CalculateMeshPosition(info, position, angle, length),
						Building.CalculateMeshRotation(angle),
						Vector3.one
					),
					lod);
				EffectManager.instance.DispatchEffect(effect, instance, spawnArea,
					Vector3.zero, 0.0f, 1f, nullAudioGroup);
				Log("Triggered bulldoze effect.");
			}
		}

		/// <summary>
		/// Send list of buildings that have specified problem flags.
		/// </summary>
		/// <param name="flags">Flags.</param>
		protected void SendProblems(ulong flags) {
			var buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
			var buildings = new List<CityWebServer.Models.BuildingInfo>();
			for(int i = 0; i < buffer.Length; i++) {
				if(((ulong)buffer[i].m_problems & flags) != 0) {
					buildings.Add(GetBuilding(i));
				}
			}
			SendJson(buildings, "ProblemBuildings");
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
				ID = id,
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
				posX = building.m_position.x,
				posY = building.m_position.y,
				posZ = building.m_position.z,
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