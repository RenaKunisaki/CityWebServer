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

	public class DistrictInfo {
		public string name;
		public uint population;
		public int popDelta;
		public int CremateCapacity;
		public int CriminalAmount;
		public int CriminalCapacity;
		public int DeadAmount;
		public int DeadCapacity;
		public int DeadCount;
		public int Education1Capacity;
		public int Education1Need;
		public int Education1Rate;
		public int Education2Capacity;
		public int Education2Need;
		public int Education2Rate;
		public int Education3Capacity;
		public int Education3Need;
		public int Education3Rate;
		public int ElectricityCapacity;
		public int ElectricityConsumption;
		public int ExportAmount;
		public int ExtraCriminals;
		public int GarbageAccumulation;
		public int GarbageAmount;
		public int GarbageCapacity;
		public int GarbagePiles;
		public int GroundPollution;
		public int HealCapacity;
		public int HeatingCapacity;
		public int HeatingConsumption;
		public int ImportAmount;
		public int IncinerationCapacity;
		public int IncomeAccumulation;
		public int LandValue;
		public int SewageAccumulation;
		public int SewageCapacity;
		public int ShelterCitizenCapacity;
		public int ShelterCitizenNumber;
		public int SickCount;
		public int Unemployment;
		public int WaterCapacity;
		public int WaterConsumption;
		public int WaterPollution;
		public int WaterStorageAmount;
		public int WaterStorageCapacity;
		public int WorkerCount;
		public int WorkplaceCount;
	}

	/// <summary>
	/// Info that's sent periodically during the game.
	/// </summary>
	public class VolatileCityInfo {
		public DateTime Time; //Current time in game
		public Boolean isNight; //Is it night? (Depends if day/night cycle enabled)
		public int simSpeed; //Selected simulation speed
		public Boolean isPaused; //Is game paused?
		public int demandR; //residential demand (0-100)
		public int demandC; //commercial demand (0-100)
		public int demandW; //workplace demand (0-100)
		public int citizenCount; //XXX what is this? it's ~8500 more than my population.
								 //it's not including tourists, either (only ~600 of those).
		public uint trafficFlow;
		public int vehicleCount;
		public int parkedCount;
		public DistrictInfo cityInfo;
	}
}