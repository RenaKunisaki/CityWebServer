namespace CityWebServer {
	/// <summary>
	/// Callback parameter for WebServer frame callbacks.
	/// Exists because Actions can only accept one parameter.
	/// </summary>
	public class FrameCallbackParam {
		public float realTimeDelta;
		public float simulationTimeDelta;
	}
}