using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CityWebServer.Extensibility;
using CityWebServer.SocketHandlers;
//using CityWebServer.Extensibility.Responses;

namespace CityWebServer.RequestHandlers {
	public class SocketRequestHandler: RequestHandlerBase {
		/** Handles `/Socket`.
		 *  Opens a WebSocket connection. Returns when the socket is closed.
		 */

		//This GUID is specified by RFC 6455 and must be appended
		//to the socket key given by the client. It's also case sensitive.
		public static readonly String KeyGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		public struct SocketMessageRawHeader {
			public byte flagsAndOpcode;
			public byte maskAndLength;
		};

		public enum SocketOpcode {
			CONTINUE = 0,
			TEXT,
			BINARY,
			RESERVED3,
			RESERVED4,
			RESERVED5,
			RESERVED6,
			RESERVED7,
			CLOSE,
			PING,
			PONG,
			RESERVED11,
			RESERVED12,
			RESERVED13,
			RESERVED14,
			RESERVED15,
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

		protected NetworkStream stream;
		//Not using ConcurrentQueue because .Net 3.5 doesn't support it
		protected Queue<String> sendQueue;
		protected readonly object sendQueueLock;

		public SocketRequestHandler()
			: base(new Guid("d33918b9-8efb-409e-9456-935669907038"),
				"Socket", "Rena", 100, "/Socket") {
		}

		public SocketRequestHandler(WebServer server, HttpRequest request, String name)
		: base(server, request, name) {
			sendQueueLock = new object();
			sendQueue = new Queue<String>();
		}

		public override void Handle() {
			/** Handle the HTTP request.
			 *  This method won't return until the socket closes!		
			 */
			this.stream = request.stream;
			Log("Connection opening");

			String key = request.headers["sec-websocket-key"];
			String respKey = Convert.ToBase64String(
				System.Security.Cryptography.SHA1.Create().ComputeHash(
					System.Text.Encoding.UTF8.GetBytes(key + KeyGuid)
				)
			);

			Log("Sending response");
			HttpResponse response = new HttpResponse(request.stream,
				statusCode: HttpStatusCode.SwitchingProtocols);
			response.AddHeader("Connection", "Upgrade");
			response.AddHeader("Upgrade", "websocket");
			response.AddHeader("Sec-WebSocket-Accept", respKey);
			response.SendHeaders();

			RunSocket();
		}

		protected void RunSocket() {
			/** Main loop, runs the WebSocket connection.
			 */

			//We don't really need to store the handlers;
			//just create them and let them call our EnqueueMessage method.
			//XXX automatically find these like WebServer does.
			Log($"Creating socket handlers (thread: {Thread.CurrentThread.Name})");
			ChirperHandler chirperHandler = new ChirperHandler(this);
			CityInfoHandler cityInfoHandler = new CityInfoHandler(this);

			Log("Waiting for messages");
			try {
				while(true) {
					if(stream.DataAvailable) HandleNextMessage();
					String msg = GetNextOutgoingMessage();
					if(msg != null) {
						byte[] buf = Encoding.UTF8.GetBytes(msg);
						SendFrame(buf);
					}
					Thread.Sleep(100);
				}
			}
			catch(ObjectDisposedException) {
				//we're done, stream is closed
				Log("Connection closed");
			}
			catch(OperationCanceledException) {
				Log("Connection closed");
			}
		}

		protected void SendFrame(byte[] data,
		SocketOpcode opcode = SocketOpcode.TEXT, bool isFIN = true) {
			/** Send a WebSocket frame to the client.
			 */
			byte[] header = new byte[14];
			header[0] = (byte)((isFIN ? 0x80 : 0) | (byte)opcode);
			int idx = 2;

			if(data.Length < 126) {
				//not bothering with masking
				header[1] = (byte)data.Length;
			}
			else if(data.Length < 65535) {
				//XXX verify byte order
				header[1] = 126;
				header[2] = (byte)(data.Length >> 8);
				header[3] = (byte)(data.Length & 0xFF);
				idx = 4;
			}
			else {
				//XXX verify byte order
				header[1] = 127;
				header[2] = (byte)((data.Length >> 56) & 0xFF);
				header[3] = (byte)((data.Length >> 48) & 0xFF);
				header[4] = (byte)((data.Length >> 40) & 0xFF);
				header[5] = (byte)((data.Length >> 32) & 0xFF);
				header[6] = (byte)((data.Length >> 24) & 0xFF);
				header[7] = (byte)((data.Length >> 16) & 0xFF);
				header[8] = (byte)((data.Length >> 8) & 0xFF);
				header[9] = (byte)(data.Length & 0xFF);
				idx = 10;
			}

			stream.Write(header, 0, idx);
			stream.Write(data, 0, data.Length);
		}

		public void EnqueueMessage(String message) {
			/** Called by handlers to add an outgoing message
			 *  to the queue to be sent to the client.
			 */
			lock(this.sendQueueLock) {
				this.sendQueue.Enqueue(message);
			}
		}

		protected String GetNextOutgoingMessage() {
			/** Retrieve the next outgoing message from the queue.
			 *  Returns null if no message is queued.
			 */
			lock(this.sendQueueLock) {
				try {
					return this.sendQueue.Dequeue();
				}
				catch(InvalidOperationException) {
					//queue is empty
					return null;
				}
			}
		}

		protected void HandleNextMessage() {
			/** Get the next message from the client and deal with it.
			 *  This is called once we know a message is available.
			 */
			try {
				var reader = new JsonFx.Json.JsonReader();
				String message = ReadMessage(request);
				var input = reader.Read<Dictionary<string, object>>(message);
				Log($"message: {input}");
				//XXX pass message on to appropriate handler.
			}
			catch(Exception ex) {
				if(ex is ObjectDisposedException
				|| ex is OperationCanceledException) {
					throw;
				}
				Log($"Error handling socket msg: {ex}");
			}
		}

		private String ReadMessage(HttpRequest req) {
			/** Read the raw WebSocket message from the socket.
			 *  Throws OperationCanceledException or ObjectDisposedException
			 *  if the socket was closed while reading.		 
			 */
			byte[] bufMsg = new byte[16384];
			int bufMsgPos = 0;
			String message = "";
			while(true) {
				//Read header
				byte[] bufHeader = new byte[32];
				SocketMessageHeader header = ReadHeader(req, bufHeader);
				//Log(
				//$"Got WebSock msg header ({header.headerLength} bytes): op={header.opcode}, " +
				//$"FIN={header.isFIN}, mask={header.isMask}, " +
				//$"length={header.length}, key={header.maskKey[0]} " +
				//$"{header.maskKey[1]} {header.maskKey[2]} {header.maskKey[3]}");

				//XXX deal with FIN bit, different opcodes

				//Read remaining buffer after header
				//Log($"Got {header.readLength - header.headerLength} extra bytes after header");
				for(int i = header.headerLength; i < header.readLength; i++) {
					bufMsg[bufMsgPos++] = bufHeader[i];
				}

				//Read data
				while(bufMsgPos < header.length && bufMsgPos < bufMsg.Length) {
					//Log($"Have {bufMsgPos} of {header.length} bytes");
					int readLen = req.stream.Read(
						bufMsg, bufMsgPos, bufMsg.Length - bufMsgPos);
					if(readLen <= 0) throw new OperationCanceledException();
					bufMsgPos += readLen;
				}

				//Decode data
				if(header.isMask) {
					//Log("Decoding message");
					//String dbg = "";
					for(int i = 0; i < bufMsgPos; i++) {
						bufMsg[i] ^= header.maskKey[i & 3];
						//String b = bufMsg[i].ToString("X2");
						//dbg += $"{b} ";
					}
					//Log($"Decoded data: {dbg}");
				}
				//else Log("Message is not encoded");
				bufMsg[bufMsgPos] = 0; //Add null terminator

				message += System.Text.Encoding.UTF8.GetString(bufMsg);
				//WebServer.Log($"Decoded socket msg: {message}");
				if(header.isFIN) return message;
			}
		}

		private SocketMessageHeader ReadHeader(HttpRequest req, byte[] buffer) {
			/** Read the WebSocket frame header from the socket.
			 *  Throws OperationCanceledException or ObjectDisposedException
			 *  if the socket was closed while reading.		
			 */
			int idx = 0;
			//Read the header bytes.
			//XXX deal with messages < 14 bytes (header is variable length)
			//14 is the maximum possible length
			while(idx < 14) {
				int r = req.stream.Read(buffer, idx, 14 - idx);
				if(r <= 0) throw new OperationCanceledException(); //closed
				idx += r;
			}
			//WebServer.Log($"Socket got header {buffer[0]} {buffer[1]}, read {idx} bytes");
			int bufLen = idx;

			//Decode the header.
			SocketMessageRawHeader header = new SocketMessageRawHeader {
				flagsAndOpcode = buffer[0],
				maskAndLength = buffer[1],
			};
			SocketOpcode opcode = (SocketOpcode)(header.flagsAndOpcode & 0x0F);
			bool isFIN = (header.flagsAndOpcode & 0x80) != 0;
			bool isMask = (header.maskAndLength & 0x80) != 0;
			long length = header.maskAndLength & 0x7F;
			byte[] maskKey = new byte[4];

			//Decode the length.
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
			else idx = 2;
			//Log($"Message length: {length}, opcode: {opcode}, mask: {isMask} @{idx}");

			//Read the mask key.
			if(isMask) {
				maskKey[0] = buffer[idx + 0];
				maskKey[1] = buffer[idx + 1];
				maskKey[2] = buffer[idx + 2];
				maskKey[3] = buffer[idx + 3];
				//Log($"Reading mask key from buffer pos {idx}: {maskKey[0]} {maskKey[1]} {maskKey[2]} {maskKey[3]}");
				idx += 4;
			}
			//else, we don't care what maskKey is, we won't use it.

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