using System.IO;
using System.Text;

namespace CityWebServer {
	public class SocketHandlerBase {
		protected Stream stream;

		public SocketHandlerBase(Stream stream) {
			this.stream = stream;
		}

		public void SendJson<T>(T body) {
			var writer = new JsonFx.Json.JsonWriter();
			var serializedData = writer.Write(body);
			byte[] buf = Encoding.UTF8.GetBytes(serializedData);
			stream.Write(buf, 0, buf.Length);
		}
	}
}