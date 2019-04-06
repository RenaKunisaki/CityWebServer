using System;

namespace CityWebServer {
	public interface ILogAppender {
		event EventHandler<LogAppenderEventArgs> LogMessage;
	}
}