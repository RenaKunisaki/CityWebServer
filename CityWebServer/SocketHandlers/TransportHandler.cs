using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
		}

		protected void SendAll() {
			var transportManager = Singleton<TransportManager>.instance;
			if(transportManager == null) {
				SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return;
			}
			//LogMessage("got transportManager");

			//somewhere in here we're getting a null reference
			//if the game hasn't finished loading yet

			TransportLine[] lines;
			List<PublicTransportLine> lineModels;
			try {
				lines = transportManager.m_lines.m_buffer;
				lineModels = new List<PublicTransportLine>();
			}
			catch(Exception ex) {
				Log($"Error getting transport lines buffer: {ex}");
				throw;
			}

			foreach(var line in lines) {
				try {
					if(line.m_flags == TransportLine.Flags.None) { continue; }

					var passengers = line.m_passengers;
					List<PopulationGroup> passengerGroups = new List<PopulationGroup> {
						new PopulationGroup("Child", (int) passengers.m_childPassengers.m_finalCount),
						new PopulationGroup("Teen", (int) passengers.m_teenPassengers.m_finalCount),
						new PopulationGroup("YoungAdult", (int) passengers.m_youngPassengers.m_finalCount),
						new PopulationGroup("Adult", (int) passengers.m_adultPassengers.m_finalCount),
						new PopulationGroup("Senior", (int) passengers.m_seniorPassengers.m_finalCount),
						new PopulationGroup("Tourist", (int) passengers.m_touristPassengers.m_finalCount),
						new PopulationGroup("Resident", (int) passengers.m_residentPassengers.m_finalCount),
						new PopulationGroup("CarOwning", (int) passengers.m_carOwningPassengers.m_finalCount)
					};

					var stops = line.CountStops(0); // The parameter is never used.
					var vehicles = line.CountVehicles(0); // The parameter is never used.

					var lineModel = new PublicTransportLine {
						Name = $"{line.Info.name} {line.m_lineNumber}",
						StopCount = stops,
						VehicleCount = vehicles,
						Passengers = passengerGroups.ToArray(),
					};
					lineModels.Add(lineModel);
				}
				catch(System.NullReferenceException) {
					//Game isn't loaded yet.
					//XXX pinpoint where this error happens.
					SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				}
			}

			//LogMessage("Transport: ordering");
			lineModels = lineModels.OrderBy(obj => obj.Name).ToList();
			SendJson(lineModels);
		}
	}
}