using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CityWebServer.Callbacks;
using CityWebServer.Extensibility;
//using CityWebServer.Extensibility.Responses;
using CityWebServer.Model;
using CityWebServer.Models;
using CityWebServer.RequestHandlers;
using ColossalFramework;
using UnityEngine;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// Sends info about the camera.
	/// </summary>
	public class CameraHandler: SocketHandlerBase {
		protected float totalTimeDelta, updateInterval;

		public CameraHandler(SocketRequestHandler handler) :
		base(handler, "Camera") {
			totalTimeDelta = 0;
			updateInterval = 0; //seconds (0=disable)
			server.frameCallbacks.Register(Update);
			handler.RegisterMessageHandler("Camera", OnCameraMessage);
		}

		/// <summary>
		/// Handle "Camera" message from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// TODO
		/// </remarks>
		public void OnCameraMessage(ClientMessage msg) {
			string action = msg.GetString("action");
			switch(action) {
				case "get":
					SendAll();
					break;
				case "set":
					HandleMove(msg, false);
					break;
				case "move":
					HandleMove(msg, true);
					break;
				case "lookAt":
					HandleLookAt(msg);
					break;
				case "updateInterval": {
						updateInterval = msg.GetFloat("interval");
						break;
					}
				default:
					throw new ArgumentException($"Invalid method {action}");
			}
		}

		/// <summary>
		/// Called each frame to send new data to client.
		/// </summary>
		/// <param name="param">Callback parameters.</param>
		protected void Update(FrameCallbackParam param) {
			if(updateInterval <= 0) return; //reporting disabled
			totalTimeDelta += param.realTimeDelta;
			if(totalTimeDelta >= updateInterval) {
				SendAll();
				totalTimeDelta = 0;
			}
		}

		/// <summary>
		/// Send camera info.
		/// </summary>
		protected void SendAll() {
			var manager = Singleton<RenderManager>.instance;
			if(manager == null) return;
			var cameraInfo = manager.CurrentCameraInfo;
			var info = new CityWebServer.Models.Camera {
				rotation = cameraInfo.m_rotation,
				shadowRotation = cameraInfo.m_shadowRotation,
				position = cameraInfo.m_position,
				up = cameraInfo.m_up,
				right = cameraInfo.m_right,
				forward = cameraInfo.m_forward,
				near = cameraInfo.m_near,
				far = cameraInfo.m_far,
			};
			SendJson(info);
		}

		/// <summary>
		/// Handles "set" and "move" messages from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		/// <param name="relative">If set to <c>true</c> relative.</param>
		protected void HandleMove(ClientMessage msg, bool relative) {
			var cam = ToolsModifierControl.cameraController;
			cam.ClearTarget();
			Vector3 targetPos, currentPos;
			Vector2 targetAngle, currentAngle;

			if(msg.HasKey("targetPosition")) {
				targetPos = msg.GetVector3("targetPosition");
			}
			else targetPos = cam.m_targetPosition;

			if(msg.HasKey("position")) {
				currentPos = msg.GetVector3("position");
			}
			else currentPos = cam.m_currentPosition;

			if(msg.HasKey("targetAngle")) {
				targetAngle = msg.GetVector2("targetAngle");
			}
			else targetAngle = cam.m_targetAngle;

			if(msg.HasKey("angle")) {
				currentAngle = msg.GetVector2("angle");
			}
			else currentAngle = cam.m_currentAngle;

			if(!relative) {
				cam.m_targetPosition.Set(0, 0, 0);
				cam.m_currentPosition.Set(0, 0, 0);
				cam.m_targetAngle.Set(0, 0);
				cam.m_currentAngle.Set(0, 0);
			}
			cam.m_targetPosition += targetPos;
			cam.m_currentPosition += currentPos;
			cam.m_targetAngle += targetAngle;
			cam.m_currentAngle += currentAngle;
		}

		/// <summary>
		/// Handles "lookAt" message from client.
		/// </summary>
		/// <param name="msg">Message.</param>
		protected void HandleLookAt(ClientMessage msg) {
			Vector3 pos;
			InstanceID id = default(InstanceID);
			var cam = ToolsModifierControl.cameraController;
			cam.ClearTarget();
			if(msg.HasKey("building")) {
				id.Building = (ushort)msg.GetInt("building");
				pos = BuildingManager.instance.m_buildings.m_buffer[id.Building].m_position;
			}
			else if(msg.HasKey("vehicle")) {
				id.Vehicle = (ushort)msg.GetInt("vehicle");
				pos = VehicleManager.instance.m_vehicles.m_buffer[id.Vehicle].GetLastFramePosition();
			}
			else if(msg.HasKey("parkedVehicle")) {
				id.ParkedVehicle = (ushort)msg.GetInt("parkedVehicle");
				pos = VehicleManager.instance.m_parkedVehicles.m_buffer[id.ParkedVehicle].m_position;
			}
			else if(msg.HasKey("segment")) {
				id.NetSegment = (ushort)msg.GetInt("segment");
				pos = NetManager.instance.m_segments.m_buffer[id.NetSegment].m_bounds.center;
			}
			else if(msg.HasKey("node")) {
				id.NetNode = (ushort)msg.GetInt("node");
				pos = NetManager.instance.m_nodes.m_buffer[id.NetNode].m_position;
			}
			else if(msg.HasKey("citizenInstance")) {
				id.CitizenInstance = (ushort)msg.GetInt("citizenInstance");
				pos = CitizenManager.instance.m_instances.m_buffer[id.CitizenInstance].GetLastFramePosition();
			}
			else if(msg.HasKey("position")) {
				cam.m_targetPosition = msg.GetVector3("position");
				return;
			}
			else {
				throw new ArgumentException("No target specified");
			}
			bool zoom = !msg.HasKey("zoom") || msg.GetBool("zoom");
			bool openPanel = !msg.HasKey("openInfoPanel") || msg.GetBool("openInfoPanel");
			cam.SetTarget(id, pos, zoom);
			if(openPanel) {
				SimulationManager.instance.m_ThreadingWrapper.QueueMainThread(() => {
					DefaultTool.OpenWorldInfoPanel(id, pos);
				});
			}
		}
	}
}