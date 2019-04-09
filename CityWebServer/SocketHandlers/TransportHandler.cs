using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using JetBrains.Annotations;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about public transport.
	/// </summary>
	public class TransportHandler: SocketHandlerBase {
		public TransportHandler(SocketRequestHandler handler) :
		base(handler, "Transport") {
			handler.RegisterMessageHandler("Transport", OnClientMessage);
		}

		/// <summary>
		/// Handle "Transport" message from client.
		/// </summary>
		/// <param name="_param">Parameter.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: line ID => get info about specified line
		/// <c>list</c>: (anything) => get list of valid IDs
		/// </remarks>
		public void OnClientMessage(SocketMessageHandlerParam _param) {
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
					SendLine((int)id);
					break;
				case "list":
					SendList();
					break;
				default:
					SendErrorResponse($"Transport has no method '{key}'");
					break;
			}
		}

		/// <summary>
		/// Send a list of valid line IDs.
		/// </summary>
		protected void SendList() {
			var transportManager = Singleton<TransportManager>.instance;
			if(transportManager == null) {
				SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return;
			}
			//somewhere in here we're getting a null reference
			//if the game hasn't finished loading yet

			List<int> ids = new List<int>();
			TransportLine[] lines;
			try {
				lines = transportManager.m_lines.m_buffer;
			}
			catch(Exception ex) {
				Log($"Error getting transport lines buffer: {ex}");
				throw;
			}

			for(int i = 0; i < lines.Length; i++) {
				var line = lines[i];
				if(line.m_flags == TransportLine.Flags.None) continue;
				ids.Add(i);
			}

			//LogMessage("Transport: ordering");
			//lineModels = lineModels.OrderBy(obj => obj.Name).ToList();
			SendJson(ids, "TransportIDs");
		}

		/// <summary>
		/// Send info about a transport line.
		/// </summary>
		/// <param name="id">Line ID.</param>
		protected void SendLine(int id) {
			SendJson(GetLine(id));
		}

		/// <summary>
		/// Get info about specified line by ID.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="id">Line ID.</param>
		protected CityWebServer.Models.PublicTransportLine GetLine(int id) {
			var transportManager = Singleton<TransportManager>.instance;
			var instanceManager = Singleton<InstanceManager>.instance;
			if(transportManager == null || instanceManager == null) {
				//SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return null;
			}
			var line = transportManager.m_lines.m_buffer[id];
			if(line.m_flags == TransportLine.Flags.None) return null;

			var passengers = line.m_passengers;
			line.GetActive(out bool day, out bool night);
			var color = line.GetColor();

			return new PublicTransportLine {
				//name = line.Info.name,
				name = transportManager.GetLineName((ushort)id),
				type = line.Info.name,
				lineNumber = line.m_lineNumber,
				vehicleCount = line.CountVehicles(0), // The parameter is never used.
				stopCount = line.CountStops(0), // The parameter is never used.
				complete = line.Complete,
				activeDay = day,
				activeNight = night,
				color = new float[] { color.r, color.g, color.b, color.a },
				ticketPrice = line.m_ticketPrice,
				totalLength = line.m_totalLength,
				maintenanceCostPerVehicle = line.Info.m_maintenanceCostPerVehicle,
				passengers = new Dictionary<string, uint> {
					{"Child", passengers.m_childPassengers.m_finalCount},
					{"Teen", passengers.m_teenPassengers.m_finalCount},
					{"YoungAdult", passengers.m_youngPassengers.m_finalCount},
					{"Adult", passengers.m_adultPassengers.m_finalCount},
					{"Senior", passengers.m_seniorPassengers.m_finalCount},
					{"Tourist", passengers.m_touristPassengers.m_finalCount},
					{"Resident", passengers.m_residentPassengers.m_finalCount},
					{"CarOwning", passengers.m_carOwningPassengers.m_finalCount},
				},
			};
		}
	}
}