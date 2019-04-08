using System;
using System.Collections.Generic;

namespace CityWebServer.Models {
	public class PublicTransportLine {
		public string name;
		public string type;
		public int lineNumber;
		public int vehicleCount;
		public int stopCount;
		public bool complete;
		public bool activeDay, activeNight;
		public float[] color;
		public ushort ticketPrice;
		public float totalLength;
		public int maintenanceCostPerVehicle;
		public Dictionary<string, uint> passengers;
	}
}