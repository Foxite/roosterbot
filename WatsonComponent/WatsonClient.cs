using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IBM.WatsonDeveloperCloud.Assistant.v2;
using IBM.WatsonDeveloperCloud.Assistant.v2.Model;
using IBM.WatsonDeveloperCloud.Util;
using RoosterBot;

namespace WatsonComponent {
	public class WatsonClient {
		internal static readonly string VersionDate = "2019-04-24";
		private const string LogTag = "Watson";
		private readonly string AssistantId;
		private AssistantService m_Assistant;
		private DiscordSocketClient m_Client;

		public WatsonClient(DiscordSocketClient client, string apiKey, string assistantId) {
			AssistantId = assistantId;
			TokenOptions ibmToken = new TokenOptions() {
				IamApiKey = apiKey,
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
						Logger.Debug(LogTag, $"Selecting from {intents.Count} intents.");
						for (int i = 1; i < intents.Count; i++) {
							if (maxConfidence.Confidence < intents[i].Confidence) {
								maxConfidence = intents[i];
							}
						}
						Logger.Debug(LogTag, $"Selected {maxConfidence.Intent}");
						string response = "Result: " + maxConfidence.Intent + "\nEntities:\n";
						string params_ = "";
						foreach (RuntimeEntity entity in result.Output.Entities) {
							response += $"- {entity.Entity}: {entity.Value}\n";

							for (int i = 0; i < entity.Location.Count; i++) {
								// The C# API says this is a nullable long, but the API documentation says its an "integer[]"
								// Even though it is literally impossible to have a string longer than (2^31-1)/2 (about 1 billion) characters (https://stackoverflow.com/a/140749/3141917),
								//  which is a length that easily fits into an Int32.
								// And to top it off, they made it nullable as well.
								// Who knows why? God, and maybe the programmers at IBM as well, but don't count on it.
								int? start = (int?) entity.Location[i];
								int? end = (int?) entity.Location[++i];
								if (start.HasValue && end.HasValue) {
									params_ += " " + input.Substring(start.Value, end.Value - start.Value);
								} else {
									Logger.Error(LogTag, $"Entity {entity.Entity}: {entity.Value} was skipped: Start or end is null");
								}
							}
						}
						response += $"\nParams: {params_}\n";
						response += $"Converted: `!{maxConfidence.Intent}{params_}`";
						Logger.Debug(LogTag, $"Natlang command `{input}` was converted into `!{maxConfidence.Intent} {params_}`");

						await message.Channel.SendMessageAsync(response);
					} else {
						Logger.Debug(LogTag, $"Natlang command `{input}` was not recognized.");
						await Util.AddReaction(message, "❓");
					}
				} catch (Exception e) {
					Logger.Error(LogTag, "That didn't work.", e);
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
