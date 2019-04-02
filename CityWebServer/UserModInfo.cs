using System;
using ICities;

namespace CityWebServer
{
    public class UserModInfo : IUserMod
    {
        public String Name
        {
            get { return "Integrated Web Server V2"; }
        }

        public String Description
        {
            get { return "Access live game data in a web browser."; }
        }
    }
}