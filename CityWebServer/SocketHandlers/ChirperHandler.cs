using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CityWebServer.Extensibility;
using CityWebServer.Helpers;
using CityWebServer.Models;
using ColossalFramework;
using ICities;
using JetBrains.Annotations;

namespace CityWebServer.RequestHandlers {
	[UsedImplicitly]
	public class ChirperHandler: SocketHandlerBase {
		/** Pushes new Chirper messages to client.
		 */
		private readonly MessageManager messageManager;
		private List<ChirperMessage> messages;

		public ChirperHandler(SocketRequestHandler handler) :
		base(handler, "Chirper") {
			Log("ChirperHandler created");

			//Since we aren't inheriting IUserMod, we have to manually
			//add our methods to the MessageManager.
			messageManager = Singleton<MessageManager>.instance;
			messageManager.m_messagesUpdated += OnMessagesUpdated;
			messageManager.m_newMessages += OnNewMessage;
			messages = new List<ChirperMessage>();
		}

		#region MessageManager callbacks

		public void OnMessagesUpdated() {
			/** Invoked when the Chirper synchronize messages
			 * (after loading a save i.e)
			 */
			DateTime now = Singleton<SimulationManager>.instance.m_currentGameTime;
			try {
				var msgs = messageManager.GetRecentMessages();
				messages = msgs.Select(obj => new ChirperMessage {
					SenderID = (int)obj.GetSenderID(),
					SenderName = obj.GetSenderName(),
					Text = obj.GetText(),
					Time = now,
					//no way to get the actual time of the message unfortunately
				}).ToList();
				SendJson(messages);
			}
			catch(Exception ex) {
				Log($"OnMessagesUpdated: {ex}");
			}
		}

		public void OnNewMessage(IChirperMessage message) {
			/** Invoked when the Chirper receives a new message
			 */
			try {
				var msg = new ChirperMessage {
					SenderID = (int)message.senderID,
					SenderName = message.senderName,
					Text = message.text,
					Time = Singleton<SimulationManager>.instance.m_currentGameTime,
				};
				messages.Add(msg);
				SendJson(msg);
			}
			catch(Exception ex) {
				Log($"OnNewMessage: {ex}");
			}
		}

		#endregion MessageManager callbacks
	} //class
} //namespace
