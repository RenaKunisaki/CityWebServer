﻿using System;
using System.Collections.Generic;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends problem notifications to client.
	/// </summary>
	public class NotificationHandler: SocketHandlerBase {
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

		public static Dictionary<Notification.Problem, string> ProblemFlags {
			get => problemFlags;
			set => problemFlags = value;
		}

		protected float totalTimeDelta, updateInterval;

		public NotificationHandler(SocketRequestHandler handler) :
		base(handler, "Notifications") {
			totalTimeDelta = 0;
			updateInterval = 5; //seconds
			SendCounts();
			server.frameCallbacks.Register(Update);
		}

		/// <summary>
		/// Send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(FrameCallbackParam param) {
			totalTimeDelta += param.realTimeDelta;
			if(totalTimeDelta < updateInterval) {
				//XXX update when info actually changes.
				return;
			}
			totalTimeDelta = 0;
			SendCounts();
		}

		/// <summary>
		/// Send count of each problem type.
		/// </summary>
		protected void SendCounts() {
			if(NotificationManager.instance == null) {
				SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return;
			}

			//don't use NotificationManager.instance.m_groupData here
			//because it defines the visual groups, which overlap
			//(for when several icons combine while zoomed out)
			//meaning the counts are inflated.

			var problemCount = new Dictionary<string, int>();
			//var groups = NotificationManager.instance.m_groupData;
			var buffer = BuildingManager.instance.m_buildings.m_buffer;
			var buildings = new List<CityWebServer.Models.BuildingInfo>();
			for(int i = 0; i < buffer.Length; i++) {
				if(buffer[i].m_problems != Notification.Problem.None) {
					var problems = buffer[i].m_problems;
					foreach(KeyValuePair<Notification.Problem, String> flag in problemFlags) {
						if(((long)problems & (long)flag.Key) != 0) {
							if(!problemCount.ContainsKey(flag.Value)) {
								problemCount[flag.Value] = 1;
							}
							else problemCount[flag.Value]++;
						}
					}
				}
			}
			SendJson(problemCount, "ProblemCounts");
		}
	}
}