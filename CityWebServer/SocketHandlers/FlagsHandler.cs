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
				["Notification.Problem"] = new Dictionary<String, ulong> {
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
		}
	}
}
