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
		private readonly UserConfigService m_UCS;
		private readonly ResourceService m_Resources;
		private readonly CommandService m_CommandService;
		private readonly WatsonClient m_Watson;

		public WatsonHandler(DiscordSocketClient client, UserConfigService ucs, GuildConfigService guildConfig, WatsonClient watson, CommandService commandService, ResourceService resources) {
			m_Discord = client;
			m_GCS = guildConfig;
			m_UCS = ucs;
			m_Watson = watson;
			m_CommandService = commandService;
			m_Resources = resources;

			m_Discord.MessageReceived += OnMessageReceived;
		}

		private Task OnMessageReceived(SocketMessage socketMsg) {
			_ = Task.Run(async () => {
				// TODO (refactor) This is a mess
				// I know this will crash the CLR if it throws an exception, although I can hardly do all this work on the gateway thread.
				try {
					if (socketMsg is SocketUserMessage msg && !msg.Author.IsBot) {
						bool process = false;
						int argPos = 0;
						IGuild guild = msg.Channel is IGuildChannel guildChannel ? guildChannel.Guild : msg.Author.MutualGuilds.First();
						GuildConfig guildConfig = await m_GCS.GetConfigAsync(guild);
						string commandPrefix = guildConfig.CommandPrefix;

						if (msg.Channel is IGuildChannel) { // If in guild: Message starts with mention to bot
							if (msg.HasMentionPrefix(m_Discord.CurrentUser, ref argPos)) {
								process = true;
							}
						} else if (msg.Author.MutualGuilds.Any()) {
							if (!msg.Content.StartsWith(commandPrefix)) {
								process = true;
							}
						}

						if (process) {
							UserConfig userConfig = await m_UCS.GetConfigAsync(msg.Author);
							CommandResponsePair? crp = userConfig.GetResponse(msg);

							var context = new RoosterCommandContext(m_Discord, msg, crp == null ? null : (await socketMsg.Channel.GetMessageAsync(crp.ResponseId) as IUserMessage), userConfig, guildConfig);

							Logger.Info("WatsonComponent", $"Processing natlang command: {context.ToString()}");
							IDisposable typingState = context.Channel.EnterTypingState();

							string? returnMessage = null;
							try {
								string input = context.Message.Content.Substring(argPos);
								if (input.Contains("\n") || input.Contains("\r") || input.Contains("\t")) {
									returnMessage = Util.Error + m_Resources.GetString(context.Culture, "WatsonClient_ProcessCommandAsync_NoExtraLinesOrTabs");
									return;
								}

								string? result = m_Watson.ConvertCommand(input);

								if (result != null) {
									await m_CommandService.ExecuteAsync(context, result, Program.Instance.Components.Services);
									// AddResponse will be handled by PostCommandHandler.
								} else {
									returnMessage = Util.Unknown + m_Resources.GetString(context.Culture, "WatsonClient_CommandNotUnderstood");
								}
							} catch (WatsonException e) {
								Logger.Error("Watson", $"Caught an exception while handling natlang command: {context}", e);
							} finally {
								if (typingState != null) {
									typingState.Dispose();
								}

								if (returnMessage != null) {
									IUserMessage returnedDiscordMessage = await context.Channel.SendMessageAsync(returnMessage);
									await userConfig.SetResponseAsync(context.Message, returnedDiscordMessage);
								}
							}
						}
					}
				} catch (Exception e) {
					Logger.Critical("WatsonHandler", "An unhandled exception was thrown in WatsonHandler. This is going to crash the CLR.", e);
					throw;
				}
			});
			return Task.CompletedTask;
		}
	}
}
