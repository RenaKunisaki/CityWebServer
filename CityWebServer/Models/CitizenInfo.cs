using System;

namespace CityWebServer.Models {
	public class CitizenInfo {
		public string name;
		public int age;
		public bool arrested;
		public int badHealth;
		public bool collapsed;
		public bool criminal;
		public bool dead;
		public int education;
		public int health;
		public byte family;
		public uint homeBuilding;
		public uint instance;
		public uint parkedVehicle;
		public bool sick;
		public int unemployed;
		public uint vehicle;
		public uint visitBuilding;
		public byte wellbeing;
		public uint workBuilding;
		public int wealth;
		//CitienInfo
		public byte agePhase;
		public byte gender; //I hope there are fewer than 256
		public float height;
		public string subCulture;
		public string thumbnail;
	}
}