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
}