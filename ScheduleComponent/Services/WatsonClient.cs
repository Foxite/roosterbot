using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IBM.WatsonDeveloperCloud.Assistant.v2;
using IBM.WatsonDeveloperCloud.Assistant.v2.Model;
using IBM.WatsonDeveloperCloud.Util;
using RoosterBot;

namespace ScheduleComponent.Services {
	public class WatsonClient {
		internal const string VersionDate = "2019-04-24";
		private const string AssistantId = "9cdb7736-dbaf-4e40-949b-c3635dc41361";
		private const string LogTag = "Watson";
		private AssistantService m_Assistant;
		private DiscordSocketClient m_Client;

		public WatsonClient(DiscordSocketClient client) {
			TokenOptions ibmToken = new TokenOptions() {
				IamApiKey = "SvawLQ8YlHv41jikWYpn5jt67GHtls5mkoOT7c_74C4J",
				ServiceUrl = "https://gateway-lon.watsonplatform.net/assistant/api"
			};
			m_Assistant = new AssistantService(ibmToken, VersionDate);
			m_Client = client;
		}

		public async Task ProcessCommandAsync(IUserMessage message, string input) {
			await Task.Run(async () => {
				string sessionId = null;
				IDisposable typingState = null;
				try {
					typingState = message.Channel.EnterTypingState();
					sessionId = m_Assistant.CreateSession(AssistantId).SessionId;
					
					MessageResponse result = m_Assistant.Message(AssistantId, sessionId, new MessageRequest() { Input = new MessageInput() { Text = input } });

					List<RuntimeIntent> intents = result.Output.Intents;
					if (intents.Count != 0) {
						RuntimeIntent maxConfidence = intents[0];
						for (int i = 1; i < intents.Count; i++) {
							if (maxConfidence.Confidence < intents[i].Confidence) {
								maxConfidence = intents[i];
							}
						}
						string response = "Result: " + maxConfidence.Intent;
						foreach (RuntimeEntity entity in result.Output.Entities) {
							response += $"\n{entity.Entity}: {entity.Value}";
							if (entity.Groups != null) {
								response += " (";
								for (int i = 0; i < entity.Groups.Count; i++) {
									response += (entity.Groups[i]?.Group ?? "null") + ", ";
								}
								response += ")";
							}
						}
						await message.Channel.SendMessageAsync(response);
					} else {
						await Util.AddReaction(message, "❓");
					}
				} catch (Exception e) {
					Logger.Log(LogSeverity.Error, LogTag, "That didn't work.", e);
				} finally {
					if (sessionId != null) {
						m_Assistant.DeleteSession(AssistantId, sessionId);
					}
					if (typingState != null) {
						typingState.Dispose();
					}
				}
			});
		}
	}
}
