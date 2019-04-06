using System;
using System.Net;
using System.Threading;
using CityWebServer.Extensibility;
using CityWebServer.Extensibility.Responses;

namespace CityWebServer.RequestHandlers {
	public class SocketRequestHandler: RequestHandlerBase {
		/** Handles `/Socket`.
		 *  Returns a WebSocket connection.
		 */

		public struct SocketMessageRawHeader {
			public byte flagsAndOpcode;
			public byte maskAndLength;
		};

		public enum SocketOpcode {
			CONTINUE = 0,
			TEXT,
			BINARY,
			//3-7: reserved
			CLOSE = 8,
			PING,
			PONG,
		};

		public class SocketMessageHeader {
			public SocketOpcode opcode;
			public bool isFIN;
			public bool isMask;
			public long length;
			public byte[] maskKey;
			public int headerLength;
			public int readLength;
		}

		public SocketRequestHandler(IWebServer server)
			: base(server, new Guid("d33918b9-8efb-409e-9456-935669907038"),
				"Socket", "Rena", 100, "/Socket") {
		}

		public override void Handle(HttpRequest request) {
			this.request = request;
			WebServer.Log("Socket connection opening");
			//ctx.AcceptWebSocketAsync() //lol too easy

			/*
			SocketResponseFormatter resp = new SocketResponseFormatter(request);
			resp.WriteContent(response);

			//System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream);

			WebServer.Log("Socket waiting for messages");
			try {
				while(true) {
					try {
						var reader = new JsonFx.Json.JsonReader();
						WebServer.Log("ReadMessage");
						String message = ReadMessage(request);
						WebServer.Log($"Read socket req: {message}");
						var input = reader.Read(message);
						WebServer.Log($"Decoded socket req: {input}");
					}
					catch(Exception ex) {
						if(ex is ObjectDisposedException) {
							throw;
						}
						WebServer.Log($"Error handling socket msg: {ex}");
					}
				}
			}
			catch(ObjectDisposedException) {
				//we're done, stream is closed
				WebServer.Log("Socket connection closed");
			}
			*/
		}

		private String ReadMessage(HttpRequest request) {
			byte[] bufMsg = new byte[16384];
			int bufMsgPos = 0;
			String message = "";
			while(true) {
				//Read header
				byte[] bufHeader = new byte[32];
				SocketMessageHeader header = ReadHeader(request, bufHeader);
				WebServer.Log(
					$"Got WebSock msg header: op={header.opcode}, " +
					$"FIN={header.isFIN}, mask={header.isMask}, " +
					$"length={header.length}, key={header.maskKey[0]} " +
					$"{header.maskKey[1]} {header.maskKey[2]} {header.maskKey[3]}");

				//Read remaining buffer after header
				for(int i = header.headerLength; i < header.readLength; i++) {
					bufMsg[bufMsgPos++] = bufHeader[i];
				}

				//Read data
				while(bufMsgPos < header.length) {
					int readLen = request.stream.Read(
						bufMsg, bufMsgPos, bufMsg.Length - bufMsgPos);
					bufMsgPos += readLen;
				}

				//Decode data
				if(header.isMask) {
					for(int i = 0; i < bufMsgPos; i++) {
						bufMsg[i] ^= header.maskKey[i & 3];
					}
				}
				message += System.Text.Encoding.UTF8.GetString(bufMsg);
				WebServer.Log($"Decoded socket msg: {message}");
				if(header.isFIN) return message;
			}
		}

		private SocketMessageHeader ReadHeader(HttpRequest request, byte[] buffer) {
			int idx = 0;
			//XXX deal with messages < 14 bytes (header is variable length)
			while(idx < 14) {
				Thread.Sleep(100);
				WebServer.Log($"Socket read header {idx}");
				int r = request.stream.Read(buffer, idx, 14 - idx);
				if(r == 0) {
					//socket is closed
				}
				idx += r;
			}
			WebServer.Log($"Socket got header {buffer[0]} {buffer[1]}");
			int bufLen = idx;

			SocketMessageRawHeader header = new SocketMessageRawHeader {
				flagsAndOpcode = buffer[0],
				maskAndLength = buffer[1],
			};
			SocketOpcode opcode = (SocketOpcode)(header.flagsAndOpcode & 0x0F);
			bool isFIN = (header.flagsAndOpcode & 0x80) != 0;
			bool isMask = (header.maskAndLength & 0x80) != 0;
			long length = header.maskAndLength & 0x7F;
			byte[] maskKey = new byte[4];
			if(length == 126) { //XXX verify byte order
				length = (buffer[2] << 8) | buffer[3];
				idx = 4;
			}
			else if(length == 127) { //XXX verify byte order
				length =
					(buffer[2] << 56) |
					(buffer[3] << 48) |
					(buffer[4] << 40) |
					(buffer[5] << 32) |
					(buffer[6] << 24) |
					(buffer[7] << 16) |
					(buffer[8] << 8) |
					 buffer[9];
				idx = 10;
			}
			if(isMask) {
				maskKey[0] = buffer[idx + 0];
				maskKey[1] = buffer[idx + 1];
				maskKey[2] = buffer[idx + 2];
				maskKey[3] = buffer[idx + 3];
				idx += 4;
			}

			return new SocketMessageHeader {
				opcode = opcode,
				isFIN = isFIN,
				isMask = isMask,
				length = length,
				maskKey = maskKey,
				headerLength = idx,
				readLength = bufLen,
			};
		}
	}
}