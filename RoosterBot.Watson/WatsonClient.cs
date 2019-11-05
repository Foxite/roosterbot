using System;
using System.Collections.Generic;
using IBM.WatsonDeveloperCloud.Assistant.v2;
using IBM.WatsonDeveloperCloud.Assistant.v2.Model;
using IBM.WatsonDeveloperCloud.Util;

namespace RoosterBot.Watson {
	public class WatsonClient {
		internal const string VersionDate = "2019-04-24";
		private const string LogTag = "Watson";
		private readonly string m_AssistantId;
		private readonly AssistantService m_Assistant;

		public WatsonClient(string apiKey, string assistantId) {
			m_AssistantId = assistantId;
			TokenOptions ibmToken = new TokenOptions() {
				IamApiKey = apiKey,
				ServiceUrl = "https://gateway-lon.watsonplatform.net/assistant/api"
			};
			m_Assistant = new AssistantService(ibmToken, VersionDate);
		}

		public string? ConvertCommandAsync(string input) {
			string? sessionId = null;
			try {
				sessionId = m_Assistant.CreateSession(m_AssistantId).SessionId;

				MessageResponse result = m_Assistant.Message(m_AssistantId, sessionId, new MessageRequest() { Input = new MessageInput() { Text = input } });

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
					string params_ = "";
					foreach (RuntimeEntity entity in result.Output.Entities) {
						for (int i = 0; i < entity.Location.Count; i++) {
							// The C# API says this is a nullable long, but the API documentation says its an "integer[]"
							// Even though it is literally impossible to have a string longer than Int32.MaxValue characters, because an array can only have that much items.
							// And to top it off, they made it nullable as well.
							// Who knows why? God, and maybe the programmers at IBM as well, but don't count on it.
							int? start = (int?) entity.Location[i];
							int? end = (int?) entity.Location[++i];
							if (start.HasValue && end.HasValue) {
								string entityValue = input[start.Value..end.Value];
								// Fix for weekday
								// For questions, please go back in time to 26 april 2019 and ask my past self.
								if (entityValue.StartsWith("op ")) {
									entityValue = entityValue.Substring(3);
								}

								params_ += " " + entityValue;
							} else {
								Logger.Error(LogTag, $"Entity {entity.Entity}: {entity.Value} was skipped: Start or end is null");
							}
						}
					}
					string convertedCommand = maxConfidence.Intent + params_;
					Logger.Debug(LogTag, $"Natlang command `{input}` was converted into `{convertedCommand}`");
					return convertedCommand;
				} else {
					Logger.Debug(LogTag, $"Natlang command `{input}` was not recognized.");
					return null;
				}
			} catch (Exception e) {
				throw new WatsonException($"Exception was thrown while converting {input}", e);
			} finally {
				if (sessionId != null) {
					m_Assistant.DeleteSession(m_AssistantId, sessionId);
				}
			}
		}
	}
}
