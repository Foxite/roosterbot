using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IBM.WatsonDeveloperCloud.Assistant.v2;
using IBM.WatsonDeveloperCloud.Assistant.v2.Model;
using IBM.WatsonDeveloperCloud.Util;

namespace RoosterBot.Watson {
	public class WatsonClient {
		internal static readonly string VersionDate = "2019-04-24";
		private const string LogTag = "Watson";
		private readonly string m_AssistantId;
		private readonly AssistantService m_Assistant;
		private readonly GuildCultureService m_GCS;
		private readonly ResourceService m_Resources;
		private readonly IDiscordClient m_DiscordClient;
		private readonly RoosterCommandService m_CommandService;
		private readonly CommandResponseService m_CRS;

		public WatsonClient(string apiKey, string assistantId, GuildCultureService gcs, ResourceService resources, IDiscordClient discord, RoosterCommandService commandService, CommandResponseService crs) {
			m_AssistantId = assistantId;
			TokenOptions ibmToken = new TokenOptions() {
				IamApiKey = apiKey,
				ServiceUrl = "https://gateway-lon.watsonplatform.net/assistant/api"
			};
			m_Assistant = new AssistantService(ibmToken, VersionDate);

			m_GCS = gcs;
			m_Resources = resources;
			m_DiscordClient = discord;
			m_CommandService = commandService;
			m_CRS = crs;
		}

		public async Task ProcessCommandAsync(IUserMessage message, string input) {
			if (input.Contains("\n") || input.Contains("\r") || input.Contains("\t")) {
				IGuild guild = (message.Author as SocketUser)?.MutualGuilds.First() ?? (message.Channel as IGuildChannel).Guild;
				CultureInfo culture = m_GCS.GetCultureForGuild(guild);
				await message.Channel.SendMessageAsync(Util.ErrorPrefix + m_Resources.GetString(culture, "WatsonClient_ProcessCommandAsync_NoExtraLinesOrTabs"));
				return;
			}

			string sessionId = null;
			IDisposable typingState = null;
			try {
				typingState = message.Channel.EnterTypingState();
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
								params_ += " " + FixWeekday(input.Substring(start.Value, end.Value - start.Value));
							} else {
								Logger.Error(LogTag, $"Entity {entity.Entity}: {entity.Value} was skipped: Start or end is null");
							}
						}
					}
					string convertedCommand = maxConfidence.Intent + params_;
					Logger.Debug(LogTag, $"Natlang command `{input}` was converted into `{convertedCommand}`");

					CommandResponsePair crp = m_CRS.GetResponse(message);
					RoosterCommandContext context;
					// TODO properly support command editing/deletion with Watson
					if (crp != null) {
						context = new RoosterCommandContext(m_DiscordClient, message, crp.Responses);
					} else {
						context = new RoosterCommandContext(m_DiscordClient, message, null);
					}
					
					await m_CommandService.ExecuteAsync(context, convertedCommand, Program.Instance.Components.Services);
					// AddResponse will be handled by PostCommandHandler.
				} else {
					Logger.Debug(LogTag, $"Natlang command `{input}` was not recognized.");
					await Util.AddReaction(message, "❓"); // TODO stop using reactions
				}
			} catch (Exception e) {
				Logger.Error(LogTag, "That didn't work.", e);
			} finally {
				if (sessionId != null) {
					m_Assistant.DeleteSession(m_AssistantId, sessionId);
				}
				if (typingState != null) {
					typingState.Dispose();
				}
			}
		}

		private string FixWeekday(string entityValue) {
			if (entityValue.StartsWith("op ")) {
				return entityValue.Substring(3);
			} else {
				return entityValue;
			}
		}
	}
}
