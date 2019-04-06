using System;

namespace CityWebServer {
	public class LogAppenderEventArgs: EventArgs {
		public String LogLine { get; set; }

		public LogAppenderEventArgs(String logLine) {
			LogLine = logLine;
		}
	}
}