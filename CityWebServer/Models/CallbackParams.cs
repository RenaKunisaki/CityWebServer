using System;
using System.Collections.Generic;

namespace CityWebServer.Callbacks {
	/// <summary>
	/// Callback parameter for WebServer frame callbacks.
	/// Exists because Actions can only accept one parameter.
	/// </summary>
	public class FrameCallbackParam {
		public float realTimeDelta;
		public float simulationTimeDelta;
	}

	/// <summary>
	/// Callback parameter for area unlock event.
	/// </summary>
	public class UnlockAreaCallbackParam {
		public int x;
		public int z;
	}

	/// <summary>
	/// Callback parameter for demand update event.
	/// </summary>
	public class UpdateDemandParam {
		public char which;
		public int demand;
	}

	/// <summary>
	/// Callback parameter for terrain height modified event.
	/// </summary>
	public class TerrainCallbackParam {
		public float minX, maxX, minZ, maxZ;
	}

	/// <summary>
	/// List of callbacks, that can be appended to from any thread.
	/// </summary>
	public class CallbackList<T> {
		public readonly string name;
		protected List<Action<T>> callbacks;
		private readonly object callbacksLock;

		public CallbackList(string name) {
			this.name = name;
			callbacks = new List<Action<T>>();
			callbacksLock = new object();
		}

		/// <summary>
		/// Register a callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Register(Action<T> callback) {
			lock(callbacksLock) {
				callbacks.Add(callback);
			}
		}

		/// <summary>
		/// Run all callbacks.
		/// </summary>
		/// <param name="param">Parameter to pass to callbacks.</param>
		public void Call(T param) {
			Action<T>[] actions;
			lock(callbacksLock) {
				actions = new Action<T>[callbacks.Count];
				callbacks.CopyTo(actions);
			}
			foreach(var action in actions) {
				//Log($"Calling frame callback {action}");
				try {
					action(param);
				}
				catch(Exception ex) {
					Log($"Error in {name} callback {action}: {ex}");
				}
			}
		}

		protected void Log(string msg) {
			WebServer.Log($"CallbackList({name}): {msg}");
		}
	}
}