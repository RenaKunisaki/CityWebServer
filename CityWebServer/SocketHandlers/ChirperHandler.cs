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
		private readonly MessageManager messageManager;
		private List<ChirperMessage> messages;

		public ChirperHandler(SocketRequestHandler handler) : base(handler, "Chirper") {
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
			/** Thread: Main
			 * Invoked when the Chirper synchronize messages
			 * (after loading a save i.e)
			 */
			Log("OnMessagesUpdated");
			try {
				var msgs = messageManager.GetRecentMessages();
				messages = msgs.Select(obj => new ChirperMessage {
					SenderID = (int)obj.GetSenderID(),
					SenderName = obj.GetSenderName(),
					Text = obj.GetText(),
				}).ToList();
				SendJson(new Dictionary<String, List<ChirperMessage>> {
					{"Chirper", messages},
				});
			}
			catch(Exception ex) {
				Log($"OnMMessagesUpdated: {ex}");
			}
		}

		public void OnNewMessage(IChirperMessage message) {
			/** Thread: Main
			 * Invoked when the Chirper receives a new message
			 */
			Log("OnNewMessage");
			try {
				var msg = new ChirperMessage {
					SenderID = (int)message.senderID,
					SenderName = message.senderName,
					Text = message.text
				};
				messages.Add(msg);
			}
			catch(Exception ex) {
				Log($"OnMNewMessages: {ex}");
			}
		}

		#endregion MessageManager callbacks
	} //class
} //namespace
