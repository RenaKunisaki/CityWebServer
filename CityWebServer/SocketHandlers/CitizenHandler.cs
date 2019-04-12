using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Helpers;
using CityWebServer.RequestHandlers;
using ColossalFramework;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about a citizen when requested.
	/// </summary>
	public class CitizenHandler: SocketHandlerBase {
		public CitizenHandler(SocketRequestHandler handler) :
		base(handler, "Citizen") {
			handler.RegisterMessageHandler("Citizen", OnClientMessage);
		}

		/// <summary>
		/// Handle "Citizen" message from client.
		/// </summary>
		/// <param name="_param">Parameter.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// <c>get</c>: building ID => get info about specified building
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
						SendErrorResponse("Invalid citizen ID");
						return;
					}
					SendCitizen((int)id);
					break;
				default:
					SendErrorResponse($"Citizen has no method '{key}'");
					break;
			}
		}

		/// <summary>
		/// Send info about one citizen.
		/// </summary>
		/// <param name="id">Citizen ID.</param>
		protected void SendCitizen(int id) {
			SendJson(GetCitizen(id));
		}

		/// <summary>
		/// Get info about specified building by ID.
		/// </summary>
		/// <returns>The building info.</returns>
		/// <param name="id">Building ID.</param>
		protected CityWebServer.Models.CitizenInfo GetCitizen(int id) {
			Citizen citizen;
			try {
				citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[id];
			}
			catch(NullReferenceException) {
				return null;
			}
			var info = citizen.GetCitizenInfo((uint)id);
			return new CityWebServer.Models.CitizenInfo {
				name = citizen.GetName(),
				age = citizen.Age,
				arrested = citizen.Arrested,
				badHealth = citizen.BadHealth,
				collapsed = citizen.Collapsed,
				criminal = citizen.Criminal,
				dead = citizen.Dead,
				education = (int)citizen.EducationLevel,
				health = citizen.m_health,
				family = citizen.m_family,
				homeBuilding = citizen.m_homeBuilding,
				instance = citizen.m_instance,
				parkedVehicle = citizen.m_parkedVehicle,
				sick = citizen.Sick,
				unemployed = citizen.Unemployed,
				vehicle = citizen.m_vehicle,
				visitBuilding = citizen.m_visitBuilding,
				wellbeing = citizen.m_wellbeing,
				workBuilding = citizen.m_workBuilding,
				wealth = (int)citizen.WealthLevel,
				agePhase = (byte)info.m_agePhase,
				gender = (byte)info.m_gender,
				height = info.m_height,
				subCulture = info.m_subCulture.Name(),
				thumbnail = info.m_Thumbnail,
			};
		}
	}
}