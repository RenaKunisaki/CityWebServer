using System;
using System.Collections.Generic;
using CityWebServer.RequestHandlers;
using ColossalFramework;
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

		public BuildingHandler(SocketRequestHandler handler) :
		base(handler, "Building") {
			//SendAll();
			handler.RegisterMessageHandler("Building", OnClientMessage);
		}

		/// <summary>
		/// Handle "Building" message from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: building ID => get info about specified building
		/// <c>list</c>: (anything) => get list of valid IDs
		/// </remarks>
		public void OnClientMessage(ClientMessage msg) {
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
				case "destroy":
					DestroyBuilding(msg.GetInt("id"));
					break;
				case "canRebuild": {
						int id = msg.GetInt("id");
						bool ok = CanRebuild(msg.GetInt("id"), out string status);
						SendJson(new Dictionary<string, string> {
							{ "canRebuild", ok.ToString() },
							{ "id", id.ToString() },
							{ "status", status },
						});
						break;
					}
				case "rebuild": {
						int id = msg.GetInt("id");
						bool force = msg.HasKey("force") && msg.GetBool("force");
						bool ok = RebuildBuilding(msg.GetInt("id"),
							out string status, force);
						SendJson(new Dictionary<string, string> {
							{ "rebuild", ok.ToString() },
							{ "id", id.ToString() },
							{ "status", status },
						});
						break;
					}
				default:
					throw new ArgumentException($"Invalid method {action}");
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
			GiveRefund(id);
			PlayBulldozeSound(target);

			//Delete the building
			Log($"Destroy building: {id}");
			manager.ReleaseBuilding((ushort)id);
			Log($"Building {id} deleted.");
		}

		/// <summary>
		/// Gives appropriate refund for destroying specified building.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <remarks>Refund may be zero.</remarks>
		protected void GiveRefund(int id) {
			var target = BuildingManager.instance.m_buildings.m_buffer[id];
			int amount = target.Info.m_buildingAI.GetRefundAmount(
				(ushort)id, ref target);
			Log($"Destroying building {id} gives refund of {amount}");
			if(amount != 0) {
				EconomyManager.instance.AddResource(
					EconomyManager.Resource.RefundAmount, amount,
					target.Info.m_class);
			}
		}

		/// <summary>
		/// Play bulldoze sound (if enabled) for specified building.
		/// </summary>
		/// <param name="target">The building being destroyed.</param>
		protected void PlayBulldozeSound(Building target) {
			EffectInfo effect = BuildingManager.instance.m_properties.m_bulldozeEffect;
			if(effect == null) return; //effect is disabled.

			Log("Triggering bulldoze effect...");

			//var nullAudioGroup = new AudioGroup(0,
			//	new SavedFloat("Bulldoze",
			//		Settings.gameSettingsFile, 0, false));
			var instance = new InstanceID();
			var spawnArea = new EffectInfo.SpawnArea(
				//Compute where the sound should come from in the world.
				Matrix4x4.TRS(
					Building.CalculateMeshPosition(target.Info,
						target.m_position, target.m_angle, target.m_length),
					Building.CalculateMeshRotation(target.m_angle),
					Vector3.one),
				target.Info.m_lodMeshData);
			EffectManager.instance.DispatchEffect(effect, instance, spawnArea,
				Vector3.zero, 0.0f, 1f,
				AudioManager.instance.EffectGroup);
			//nullAudioGroup);
			Log("Triggered bulldoze effect.");
		}

		/// <summary>
		/// Check if we can rebuild the specified building.
		/// </summary>
		/// <returns><c>true</c> if able to rebuild.</returns>
		/// <param name="id">Building ID.</param>
		/// <param name="status">Receives status code.</param>
		protected bool CanRebuild(int id, out string status) {
			BuildingManager buildingManager = BuildingManager.instance;
			Building building = buildingManager.m_buildings.m_buffer[id];
			BuildingInfo info = building.Info;
			if(IsRico(building)) {
				Log($"Building {id} is not a city building.");
				status = "WrongType";
				return false;
			}

			var flags = Building.Flags.BurnedDown | Building.Flags.Collapsed;
			if((building.m_flags & flags) == 0) {
				Log($"Building {id} does not need rebuilding.");
				status = "NotDestroyed";
				return false;
			}

			var problems = (
				Notification.Problem.StructureVisited | //RICO ready to rebuild
				Notification.Problem.StructureVisitedService); //other ready to rebuild
			if((building.m_problems & problems) == 0) {
				Log($"Building {id} is not ready to rebuild.");
				status = "NotReady";
				return false;
			}

			int relocationCost = info.m_buildingAI.GetRelocationCost();
			Log($"Rebuilding building {id} costs {relocationCost / 100}");
			if(relocationCost >= EconomyManager.instance.InternalCashAmount) {
				Log($"Not enough money to rebuild building {id}.");
				status = "NoMoney";
				return false;
			}

			status = "OK";
			return true;
		}

		/// <summary>
		/// Attempt to rebuild the specified building.
		/// </summary>
		/// <returns><c>true</c> if rebuilt.</returns>
		/// <param name="id">Building ID.</param>
		/// <param name="status">Receives status code.</param>
		/// <param name="force">Whether to override normal restrictions.</param>
		/// <remarks>Mostly copied from https://github.com/keallu/CSL-RebuildIt/blob/master/RebuildIt/RebuildUtils.cs</remarks>
		protected bool RebuildBuilding(int id, out string status, bool force = false) {
			if(!force) {
				if(!CanRebuild(id, out string r)) {
					status = r;
					return false;
				}
			}

			BuildingManager buildingManager = BuildingManager.instance;
			Building building = buildingManager.m_buildings.m_buffer[id];
			BuildingInfo info = building.Info;

			Log($"Rebuilding building {id}...");
			if(IsRico(building)) {
				//Normally can't be rebuilt by player, but `force` overrides this.
				building.m_problems = Notification.Problem.None;
				//XXX what are Active and Completed?
				building.m_flags &= ~(Building.Flags.BurnedDown |
					Building.Flags.Collapsed | Building.Flags.Active |
					Building.Flags.Completed);
				building.m_flags |= Building.Flags.ZonesUpdated;
			}
			else {
				//Actually deduct the cost.
				int relocationCost = info.m_buildingAI.GetRelocationCost();
				EconomyManager.instance.FetchResource(
					EconomyManager.Resource.Construction,
					relocationCost, info.m_class);

				//Relocate the building to its current location, ie rebuild it.
				buildingManager.RelocateBuilding((ushort)id,
					building.m_position, building.m_angle);
				RebuildSubBuildings(id);

				//Restore whatever service the building was providing.
				int publicServiceIndex = ItemClass.GetPublicServiceIndex(info.m_class.m_service);
				if(publicServiceIndex != -1) {
					buildingManager.m_buildingDestroyed2.Disable();
					GuideManager.instance.m_serviceNotUsed[publicServiceIndex].Disable();
					GuideManager.instance.m_serviceNeeded[publicServiceIndex].Deactivate();
					CoverageManager.instance.CoverageUpdated(info.m_class.m_service,
						info.m_class.m_subService, info.m_class.m_level);
				}
			}
			Log($"Rebuilt building {id}.");
			status = "OK";
			return true;
		}

		/// <summary>
		/// Check if specified building is Residential, Industrial,
		/// Commercial, or Office.
		/// </summary>
		/// <returns><c>true</c> if one of these types, <c>false</c> otherwise.</returns>
		/// <param name="building">Building.</param>
		protected bool IsRico(Building building) {
			switch(building.Info.m_class.GetZone()) {
				case ItemClass.Zone.ResidentialHigh:
				case ItemClass.Zone.ResidentialLow:
				case ItemClass.Zone.Industrial:
				case ItemClass.Zone.CommercialHigh:
				case ItemClass.Zone.CommercialLow:
				case ItemClass.Zone.Office:
					return true;
				default:
					return false;
			}
		}

		protected void RebuildSubBuildings(int id) {
			BuildingManager buildingManager = BuildingManager.instance;
			SimulationManager simulationManager = SimulationManager.instance;
			Building building = buildingManager.m_buildings.m_buffer[id];
			BuildingInfo info = building.Info;

			if(info.m_subBuildings == null || info.m_subBuildings.Length == 0) return;
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetTRS(building.m_position,
				//This number is 1 radian in degrees.
				Quaternion.AngleAxis(building.m_angle * 57.29578f, Vector3.down),
				Vector3.one);

			BuildingInfo subBuildingInfo;
			Vector3 position;
			float angle;
			bool fixedHeight;
			for(int i = 0; i < info.m_subBuildings.Length; i++) {
				subBuildingInfo = info.m_subBuildings[i].m_buildingInfo;
				position = matrix4x.MultiplyPoint(
					info.m_subBuildings[i].m_position);
				//This number is 1 degree in radians.
				angle = info.m_subBuildings[i].m_angle * 0.0174532924f +
					building.m_angle;
				fixedHeight = info.m_subBuildings[i].m_fixedHeight;

				if(buildingManager.CreateBuilding(out ushort subBuildingId,
					ref SimulationManager.instance.m_randomizer,
					info, position, angle, 0,
					SimulationManager.instance.m_currentBuildIndex)) {
					if(fixedHeight) {
						buildingManager.m_buildings.m_buffer[subBuildingId]
						.m_flags |= Building.Flags.FixedHeight;
					}
					SimulationManager.instance.m_currentBuildIndex++;
				}

				if(id != 0 && subBuildingId != 0) {
					buildingManager.m_buildings.m_buffer[id].m_subBuilding = subBuildingId;
					buildingManager.m_buildings.m_buffer[subBuildingId].m_parentBuilding = (ushort)id;
					buildingManager.m_buildings.m_buffer[subBuildingId].m_flags |= Building.Flags.Untouchable;
					id = subBuildingId; //XXX is this right?
				}
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
				name = BuildingManager.instance.GetBuildingName(
					(ushort)id, building.Info.m_instanceID),
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