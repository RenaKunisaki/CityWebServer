using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Extensibility;
using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using ColossalFramework;

namespace CityWebServer.RequestHandlers {
	public class NotificationRequestHandler: RequestHandlerBase {
		/** Handles `/Notifications`.
		 *  Returns list of active problem notifications.
		 */

		private static Dictionary<Notification.Problem, String> problemFlags =
		new Dictionary<Notification.Problem, String>() {
			{Notification.Problem.Crime, "Crime"},
			{Notification.Problem.Death, "Death"},
			{Notification.Problem.DepotNotConnected, "DepotNotConnected"},
			{Notification.Problem.DirtyWater, "DirtyWater"},
			{Notification.Problem.Electricity, "Electricity"},
			{Notification.Problem.ElectricityNotConnected, "ElectricityNotConnected"},
			{Notification.Problem.Emptying, "Emptying"},
			{Notification.Problem.EmptyingFinished, "EmptyingFinished"},
			{Notification.Problem.Evacuating, "Evacuating"},
			{Notification.Problem.FatalProblem, "FatalProblem"},
			{Notification.Problem.Fire, "Fire"},
			{Notification.Problem.Flood, "Flood"},
			{Notification.Problem.Garbage, "Garbage"},
			{Notification.Problem.Heating, "Heating"},
			{Notification.Problem.HeatingNotConnected, "HeatingNotConnected"},
			{Notification.Problem.LandfillFull, "LandfillFull"},
			{Notification.Problem.LandValueLow, "LandValueLow"},
			{Notification.Problem.LineNotConnected, "LineNotConnected"},
			{Notification.Problem.MajorProblem, "MajorProblem"},
			{Notification.Problem.NoCustomers, "NoCustomers"},
			{Notification.Problem.NoEducatedWorkers, "NoEducatedWorkers"},
			{Notification.Problem.NoFood, "NoFood"},
			{Notification.Problem.NoFuel, "NoFuel"},
			{Notification.Problem.NoGoods, "NoGoods"},
			{Notification.Problem.NoInputProducts, "NoInputProducts"},
			{Notification.Problem.Noise, "Noise"},
			{Notification.Problem.NoMainGate, "NoMainGate"},
			{Notification.Problem.NoNaturalResources, "NoNaturalResources"},
			{Notification.Problem.NoPark, "NoPark"},
			{Notification.Problem.NoPlaceforGoods, "NoPlaceForGoods"},
			{Notification.Problem.NoResources, "NoResources"},
			{Notification.Problem.NotInIndustryArea, "NotInIndustryArea"},
			{Notification.Problem.NoWorkers, "NoWorkers"},
			{Notification.Problem.PathNotConnected, "PathNotConnected"},
			{Notification.Problem.Pollution, "Pollution"},
			{Notification.Problem.ResourceNotSelected, "ResourceNotSelected"},
			{Notification.Problem.RoadNotConnected, "RoadNotConnected"},
			{Notification.Problem.Sewage, "Sewage"},
			{Notification.Problem.Snow, "Snow"},
			{Notification.Problem.StructureDamaged, "StructureDamaged"},
			{Notification.Problem.StructureVisited, "StructureVisited"},
			{Notification.Problem.StructureVisitedService, "StructureVisitedService"},
			{Notification.Problem.TaxesTooHigh, "TaxesTooHigh"},
			{Notification.Problem.TooFewServices, "TooFewServices"},
			{Notification.Problem.TooLong, "TooLong"},
			{Notification.Problem.TrackNotConnected, "TrackNotConnected"},
			{Notification.Problem.TurnedOff, "TurnedOff"},
			{Notification.Problem.Water, "Water"},
			{Notification.Problem.WaterNotConnected, "WaterNotConnected"},
			{Notification.Problem.WrongAreaType, "WrongAreaType"},
		};

		public NotificationRequestHandler(IWebServer server)
			: base(server, new Guid("757d3b12-395b-4cb7-930a-81fd854afe24"),
				"Notifications", "Rena", 100, "/Notifications") {
		}

		public static Dictionary<Notification.Problem, string> ProblemFlags { get => problemFlags; set => problemFlags = value; }

		public override IResponseFormatter Handle(HttpListenerRequest request) {
			// TODO: Expand upon this to expose substantially more information.
			var buildingManager = Singleton<BuildingManager>.instance;
			if(buildingManager == null) {
				return new PlainTextResponseFormatter("",
					HttpStatusCode.ServiceUnavailable);
			}

			NotificationInfo info = new NotificationInfo {
				problemCount = new Dictionary<string, int>(),
			};
			foreach(var building in buildingManager.m_buildings.m_buffer) {
				if(building.m_flags == Building.Flags.None) continue;
				if(building.m_problems == Notification.Problem.None) continue;
				//IntegratedWebServer.LogMessage($"Building problems: {building.m_problems}");
				foreach(KeyValuePair<Notification.Problem, String> flag in problemFlags) {
					if(((long)building.m_problems & (long)flag.Key) != 0) {
						if(!info.problemCount.ContainsKey(flag.Value)) {
							info.problemCount[flag.Value] = 1;
						}
						else info.problemCount[flag.Value]++;
					}
				}
			}

			return JsonResponse(info);
		}
	}
}