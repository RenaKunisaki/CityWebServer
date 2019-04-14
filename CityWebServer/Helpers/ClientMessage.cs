using System;
using System.Collections.Generic;
using CityWebServer.Callbacks;
using UnityEngine;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// A message received from a socket client.
	/// </summary>
	public class ClientMessage {
		public ClientMessage(SocketMessageHandlerParam _param) {
			this.param = _param.param;
		}

		public bool HasKey(string key) {
			var p = this.param as Dictionary<string, object>;
			return p.ContainsKey(key);
		}

		public T Get<T>(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, T>;
			var v = p[key];
			if(v is Nullable && v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return v;
		}

		public Vector2 GetVector2(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, float[]>;
			var v = p[key];
			if(v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			if(v.Length != 2) {
				throw new ArgumentException($"invalid Vector2 for {key}");
			}
			return new Vector3(v[0], v[1]);
		}

		public Vector3 GetVector3(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, float[]>;
			var v = p[key];
			if(v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			if(v.Length != 3) {
				throw new ArgumentException($"invalid Vector3 for {key}");
			}
			return new Vector3(v[0], v[1], v[2]);
		}

		protected object param;
	}
}