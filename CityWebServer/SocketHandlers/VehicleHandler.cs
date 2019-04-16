using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about vehicles.
	/// </summary>
	public class VehicleHandler: SocketHandlerBase {
		public VehicleHandler(SocketRequestHandler handler) :
		base(handler, "Vehicle") {
			handler.RegisterMessageHandler("Vehicle", OnClientMessage);
		}

		/// <summary>
		/// Handle "Vehicle" message from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: line ID => get info about specified line
		/// <c>list</c>: (anything) => get list of valid IDs
		/// </remarks>
		public void OnClientMessage(ClientMessage msg) {
			string action = msg.GetString("action");
			switch(action) {
				case "get":
					SendVehicle(msg.GetInt("id"));
					break;
				case "list":
					SendList();
					break;
				default:
					throw new ArgumentException($"Invalid method {action}");
			}
		}

		/// <summary>
		/// Send a list of valid vehicle IDs.
		/// </summary>
		protected void SendList() {
			var vehicleManager = Singleton<VehicleManager>.instance;
			if(vehicleManager == null) {
				SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return;
			}
			List<int> ids = new List<int>();
			Vehicle[] vehicles;
			try {
				vehicles = vehicleManager.m_vehicles.m_buffer;
			}
			catch(Exception ex) {
				Log($"Error getting vehicles buffer: {ex}");
				throw;
			}

			for(int i = 0; i < vehicles.Length; i++) {
				var vehicle = vehicles[i];
				//for some reason there's no Vehicle.Flags.None
				if(vehicle.m_flags == 0) continue;
				ids.Add(i);
			}

			SendJson(ids, "VehicleIDs");
		}

		/// <summary>
		/// Send info about a transport line.
		/// </summary>
		/// <param name="id">Line ID.</param>
		protected void SendVehicle(int id) {
			SendJson(GetVehicle(id));
		}

		/// <summary>
		/// Get info about specified line by ID.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="id">Line ID.</param>
		protected CityWebServer.Models.Vehicle GetVehicle(int id) {
			var vehicleManager = Singleton<VehicleManager>.instance;
			if(vehicleManager == null) {
				//SendErrorResponse(HttpStatusCode.ServiceUnavailable);
				return null;
			}

			var vehicle = vehicleManager.m_vehicles.m_buffer[id];
			if(vehicle.m_flags == 0) return null;

			return new CityWebServer.Models.Vehicle {
				ID = id,
				name = vehicle.Info.name,
				citizenUnits = vehicle.m_citizenUnits,
				firstCargo = vehicle.m_firstCargo,
				flags = (uint)vehicle.m_flags,
				flags2 = (uint)vehicle.m_flags2,
				gateIndex = vehicle.m_gateIndex,
				infoIndex = vehicle.m_infoIndex,
				lastPathOffset = vehicle.m_lastPathOffset,
				leadingVehicle = vehicle.m_leadingVehicle,
				nextCargo = vehicle.m_nextCargo,
				nextGridVehicle = vehicle.m_nextGridVehicle,
				nextGuestVehicle = vehicle.m_nextGuestVehicle,
				nextLineVehicle = vehicle.m_nextLineVehicle,
				nextOwnVehicle = vehicle.m_nextOwnVehicle,
				path = vehicle.m_path,
				pathPositionIndex = vehicle.m_pathPositionIndex,
				segmentA = vehicle.m_segment.a,
				segmentB = vehicle.m_segment.b,
				sourceBuilding = vehicle.m_sourceBuilding,
				targetBuilding = vehicle.m_targetBuilding,
				targetPos0 = vehicle.m_targetPos0,
				targetPos1 = vehicle.m_targetPos1,
				targetPos2 = vehicle.m_targetPos2,
				targetPos3 = vehicle.m_targetPos3,
				touristCount = vehicle.m_touristCount,
				trailingVehicle = vehicle.m_trailingVehicle,
				transferSize = vehicle.m_transferSize,
				transferType = vehicle.m_transferType,
				transportLine = vehicle.m_transportLine,
				waitCounter = vehicle.m_waitCounter,
				waterSource = vehicle.m_waterSource,
				title = vehicle.Info.GetUncheckedLocalizedTitle(),
				acceleration = vehicle.Info.m_acceleration,
				braking = vehicle.Info.m_braking,
				maxSpeed = vehicle.Info.m_maxSpeed,
				springs = vehicle.Info.m_springs,
				turning = vehicle.Info.m_turning,
				isLargeVehicle = vehicle.Info.m_isLargeVehicle,
				maxTrailerCount = vehicle.Info.m_maxTrailerCount,
				thumbnail = vehicle.Info.m_Thumbnail,
			};
		}
	}
}