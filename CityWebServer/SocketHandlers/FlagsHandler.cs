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
	/// Sends list of flag names and values.
	/// </summary>
	public class FlagsHandler: SocketHandlerBase {

		public FlagsHandler(SocketRequestHandler handler) :
		base(handler, "Flags") {
			Update();
		}

		protected void Update() {
			Dictionary<String, Dictionary<String, ulong>> flags =
			new Dictionary<String, Dictionary<String, ulong>> {
				["Building"] = new Dictionary<String, ulong> {
					["Abandoned"] = (ulong)Building.Flags.Abandoned,
					["Active"] = (ulong)Building.Flags.Active,
					["BurnedDown"] = (ulong)Building.Flags.BurnedDown,
					["CapacityStep1"] = (ulong)Building.Flags.CapacityStep1,
					["CapacityStep2"] = (ulong)Building.Flags.CapacityStep2,
					["Collapsed"] = (ulong)Building.Flags.Collapsed,
					["Completed"] = (ulong)Building.Flags.Completed,
					["Content01"] = (ulong)Building.Flags.Content01,
					["Content01_Forbid"] = (ulong)Building.Flags.Content01_Forbid,
					["Content02"] = (ulong)Building.Flags.Content02,
					["Content02_Forbid"] = (ulong)Building.Flags.Content02_Forbid,
					["Content03"] = (ulong)Building.Flags.Content03,
					["Content03_Forbid"] = (ulong)Building.Flags.Content03_Forbid,
					["Content04"] = (ulong)Building.Flags.Content04,
					["Content04_Forbid"] = (ulong)Building.Flags.Content04_Forbid,
					["Content05"] = (ulong)Building.Flags.Content05,
					["Content05_Forbid"] = (ulong)Building.Flags.Content05_Forbid,
					["Content06"] = (ulong)Building.Flags.Content06,
					["Content06_Forbid"] = (ulong)Building.Flags.Content06_Forbid,
					["Content07"] = (ulong)Building.Flags.Content07,
					["Content07_Forbid"] = (ulong)Building.Flags.Content07_Forbid,
					["Content08"] = (ulong)Building.Flags.Content08,
					["Content08_Forbid"] = (ulong)Building.Flags.Content08_Forbid,
					["Content09"] = (ulong)Building.Flags.Content09,
					["Content09_Forbid"] = (ulong)Building.Flags.Content09_Forbid,
					["Content10"] = (ulong)Building.Flags.Content10,
					["Content10_Forbid"] = (ulong)Building.Flags.Content10_Forbid,
					["Content11"] = (ulong)Building.Flags.Content11,
					["Content11_Forbid"] = (ulong)Building.Flags.Content11_Forbid,
					["Content12"] = (ulong)Building.Flags.Content12,
					["Content12_Forbid"] = (ulong)Building.Flags.Content12_Forbid,
					["Content13"] = (ulong)Building.Flags.Content13,
					["Content13_Forbid"] = (ulong)Building.Flags.Content13_Forbid,
					["Content14"] = (ulong)Building.Flags.Content14,
					["Content14_Forbid"] = (ulong)Building.Flags.Content14_Forbid,
					["Created"] = (ulong)Building.Flags.Created,
					["CustomName"] = (ulong)Building.Flags.CustomName,
					["Deleted"] = (ulong)Building.Flags.Deleted,
					["Demolishing"] = (ulong)Building.Flags.Demolishing,
					["Downgrading"] = (ulong)Building.Flags.Downgrading,
					["Evacuating"] = (ulong)Building.Flags.Evacuating,
					["EventActive"] = (ulong)Building.Flags.EventActive,
					["Filling"] = (ulong)Building.Flags.Filling,
					["FixedHeight"] = (ulong)Building.Flags.FixedHeight,
					["Flooded"] = (ulong)Building.Flags.Flooded,
					["Hidden"] = (ulong)Building.Flags.Hidden,
					["HighDensity"] = (ulong)Building.Flags.HighDensity,
					["Historical"] = unchecked((ulong)Building.Flags.Historical),
					["Incoming"] = (ulong)Building.Flags.Incoming,
					["IncomingOutgoing"] = (ulong)Building.Flags.IncomingOutgoing,
					["LevelUpEducation"] = (ulong)Building.Flags.LevelUpEducation,
					["LevelUpLandValue"] = (ulong)Building.Flags.LevelUpLandValue,
					["Loading1"] = (ulong)Building.Flags.Loading1,
					["Loading2"] = (ulong)Building.Flags.Loading2,
					["Original"] = (ulong)Building.Flags.Original,
					["Outgoing"] = (ulong)Building.Flags.Outgoing,
					["RateReduced"] = (ulong)Building.Flags.RateReduced,
					["RoadAccessFailed"] = (ulong)Building.Flags.RoadAccessFailed,
					["SecondaryLoading"] = (ulong)Building.Flags.SecondaryLoading,
					["Untouchable"] = (ulong)Building.Flags.Untouchable,
					["Upgrading"] = (ulong)Building.Flags.Upgrading,
					["ZonesUpdated"] = (ulong)Building.Flags.ZonesUpdated,
				},
				["Problem"] = new Dictionary<String, ulong> {
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
				},
			};
			SendJson(flags, "Flags");
		}
	}
}
