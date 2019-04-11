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
		/// Handle "Canera" message from client.
		/// </summary>
		/// <param name="_param">Parameter.</param>
		/// <remarks>Expects a dict with one of the keys:
		/// TODO
		/// </remarks>
		public void OnCameraMessage(SocketMessageHandlerParam _param) {
			var param = _param.param as Dictionary<string, object>;
			var key = param.Keys.First();
			switch(key) {
				case null:
					SendErrorResponse(HttpStatusCode.BadRequest);
					break;
				case "get":
					SendAll();
					break;
				case "set": {
						//XXX allow to set more than position
						var manager = Singleton<RenderManager>.instance;
						if(manager == null) {
							SendErrorResponse(HttpStatusCode.ServiceUnavailable);
							return;
						}
						var cameraInfo = manager.CurrentCameraInfo;
						float[] vals = param["set"] as float[];
						cameraInfo.m_position = new UnityEngine.Vector3(
							vals[0], vals[1], vals[2]);
						break;
					}
				case "move": {
						//XXX allow to set more than position
						var manager = Singleton<RenderManager>.instance;
						if(manager == null) {
							SendErrorResponse(HttpStatusCode.ServiceUnavailable);
							return;
						}
						var cameraInfo = manager.CurrentCameraInfo;
						float[] vals = param["move"] as float[];
						cameraInfo.m_position += new UnityEngine.Vector3(
							vals[0], vals[1], vals[2]);
						break;
					}
				case "updateInterval": {
						float? interval = param["updateInterval"] as float?;
						if(interval == null) updateInterval = 0;
						else updateInterval = (float)interval;
						break;
					}
				default:
					SendErrorResponse($"District has no method '{key}'");
					break;
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
	}
}