using System;
using System.Collections.Generic;

namespace CityWebServer.Callbacks {
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
		/// Unregister the specified callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Unregister(Action<T> callback) {
			lock(callbacksLock) {
				callbacks.Remove(callback);
			}
		}

		/// <summary>
		/// Run all callbacks.
		/// </summary>
		/// <param name="param">Parameter to pass to callbacks.</param>
		public void Call(T param) {
			//Make a copy of the callback list, so that we
			//don't need to keep it locked while calling them.
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