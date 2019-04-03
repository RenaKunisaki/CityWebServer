using System;

namespace CityWebServer.Models
{
    public class CityInfo
    {
		public String Name;
		public String mapName;
		public String environment;
		public DateTime Time;
		public DistrictInfo GlobalDistrict;
		public DistrictInfo[] Districts;
		public Boolean isNight;
		public int simSpeed;
		public Boolean isPaused;
    }
}