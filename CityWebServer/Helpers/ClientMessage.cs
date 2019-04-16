using System;
using System.Collections.Generic;
using CityWebServer.Callbacks;
using UnityEngine;

namespace CityWebServer.SocketHandlers {
	/// <summary>
	/// A message received from a socket client.
	/// </summary>
	public class ClientMessage {
		public ClientMessage(object _param) {
			this.param = _param;
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

		public string GetString(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, object>;
			string v = p[key] as string;
			if(v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return v;
		}

		public string[] GetStringArray(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, object>;
			string[] v = p[key] as string[];
			if(v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return v;
		}

		public int GetInt(string key) {
			var p = this.param as Dictionary<string, object>;
			int? v = p[key] as int?;
			if(v == null) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return (int)v;
		}

		public float GetFloat(string key) {
			var p = this.param as Dictionary<string, object>;
			float? v = p[key] as float?;
			if(v == null) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return (float)v;
		}

		public bool GetBool(string key) {
			var p = this.param as Dictionary<string, object>;
			bool? v = p[key] as bool?;
			if(v == null) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return (bool)v;
		}

		public object GetObject(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, object>;
			object v = p[key] as object;
			if(v == null && !allowNull) {
				throw new ArgumentException($"invalid value for {key}");
			}
			return v;
		}

		public object[] GetObjectArray(string key, bool allowNull = false) {
			var p = this.param as Dictionary<string, object>;
			object[] v = p[key] as object[];
			if(v == null && !allowNull) {
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