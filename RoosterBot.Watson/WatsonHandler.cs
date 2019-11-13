using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot.Watson {
	/// <summary>
	/// Checks received messages if they start with a mention to the bot user, and calls WatsonClient to parse it.
	/// </summary>
	internal sealed class WatsonHandler {
		private readonly DiscordSocketClient m_Discord;
		private readonly GuildConfigService m_GCS;
		private readonly CommandResponseService m_CRS;
		private readonly ResourceService m_Resources;
		private readonly CommandService m_CommandService;
		private readonly WatsonClient m_Watson;

		public WatsonHandler(DiscordSocketClient client, CommandResponseService crs, GuildConfigService guildConfig, WatsonClient watson, CommandService commandService, ResourceService resources) {
			m_Discord = client;
			m_GCS = guildConfig;
			m_CRS = crs;
			m_Watson = watson;
			m_CommandService = commandService;
			m_Resources = resources;

			m_Discord.MessageReceived += async (SocketMessage socketMsg) => {
				if (socketMsg is SocketUserMessage msg && !msg.Author.IsBot) {
					bool process = false;
					int argPos = 0;
					if (msg.Channel is IGuildChannel guildChannel) { // If in guild: Message starts with mention to bot
						string commandPrefix = (await guildConfig.GetConfigAsync(guildChannel.Guild)).CommandPrefix;
						if (msg.HasMentionPrefix(m_Discord.CurrentUser, ref argPos)) {
							process = true;
						}
					} else if (msg.Author.MutualGuilds.Any()) {
						string commandPrefix = (await guildConfig.GetConfigAsync(msg.Author.MutualGuilds.First())).CommandPrefix;
						if (!msg.Content.StartsWith(commandPrefix)) {
							process = true;
						}
					}

					if (process) {
						CommandResponsePair? crp = m_CRS.GetResponse(msg);
						RoosterCommandContext context = new RoosterCommandContext(m_Discord, msg, crp?.Responses);

						// Do not await this task on the gateway thread because it can take very long.
						_ = Task.Run(async () => {
							await ProcessNaturalLanguageCommandsAsync(context, argPos);
						});
					}
				}
			};
		}

		private async Task ProcessNaturalLanguageCommandsAsync(RoosterCommandContext context, int argPos) {
			Logger.Info("WatsonComponent", $"Processing natlang command: {context.ToString()}");
			IDisposable typingState = context.Channel.EnterTypingState();

			IGuild cultureGuild;
			if (context.IsPrivate && context.User is SocketUser socketUser) {
				cultureGuild = socketUser.MutualGuilds.FirstOrDefault();
			} else {
				cultureGuild = context.Guild!;
			}
			GuildConfig guildConfig = await m_GCS.GetConfigAsync(cultureGuild);

			string? returnMessage = null;
			try {
				string input = context.Message.Content.Substring(argPos);
				if (input.Contains("\n") || input.Contains("\r") || input.Contains("\t")) {
					returnMessage = Util.Error + m_Resources.GetString(guildConfig.Culture, "WatsonClient_ProcessCommandAsync_NoExtraLinesOrTabs");
					return;
				}

				string? result = m_Watson.ConvertCommandAsync(input);

				if (result != null) {
					await m_CommandService.ExecuteAsync(context, result, Program.Instance.Components.Services);
					// AddResponse will be handled by PostCommandHandler.
				} else {
					returnMessage = Util.Unknown + m_Resources.GetString(guildConfig.Culture, "WatsonClient_CommandNotUnderstood");
				}
			} catch (WatsonException e) {
				Logger.Error("Watson", $"Caught an exception while handling natlang command: {context}", e);
			} finally {
				if (typingState != null) {
					typingState.Dispose();
				}

				if (returnMessage != null) {
					IUserMessage returnedDiscordMessage = await context.Channel.SendMessageAsync(returnMessage);
					if (context.Responses == null) {
						m_CRS.AddResponse(context.Message, returnedDiscordMessage);
					} else {
						m_CRS.ModifyResponse(context.Message, new[] { returnedDiscordMessage });
					}
				}
			}
		}
	}
}
