using System;

namespace CityWebServer.Models {
	/// <summary>
	/// Info that's sent only at startup or when it changes.
	/// </summary>
	public class InitialCityInfo {
		public String Name; //City name
		public String mapName; //Map name
		public String environment; //Map style, eg "Europe"
		public Boolean invertTraffic; //Traffic drives on left?
		public DateTime startingDateTime; //When city was created

		//public DistrictInfo GlobalDistrict;
		//public DistrictInfo[] Districts;
		public Boolean[] isTileUnlocked;
	}

	/// <summary>
	/// Info that's sent periodically during the game.
	/// </summary>
	public class VolatileCityInfo {
		public DateTime Time; //Current time in game
		public Boolean isNight; //Is it night? (Depends if day/night cycle enabled)
		public int simSpeed; //Selected simulation speed
		public Boolean isPaused; //Is game paused?
		public int citizenCount; //XXX what is this? it's ~8500 more than my population.
								 //it's not including tourists, either (only ~600 of those).
		public uint trafficFlow;
		public int vehicleCount;
		public int parkedCount;
	}
}