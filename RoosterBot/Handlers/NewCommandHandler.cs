using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class NewCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly ConfigService m_ConfigService;
		private readonly GuildConfigService m_GCS;

		internal NewCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService configService, GuildConfigService gcs) {
			m_Client = client;
			m_Commands = commands;
			m_ConfigService = configService;
			m_GCS = gcs;
			m_Client.MessageReceived += HandleNewCommand;
		}

		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			string prefix;
			if (socketMessage.Channel is IGuildChannel guildChannel) {
				prefix = (await m_GCS.GetConfigAsync(guildChannel.Guild)).CommandPrefix;
			} else {
				prefix = m_ConfigService.DefaultCommandPrefix;
			}

			if (socketMessage is IUserMessage userMessage && m_Commands.IsMessageCommand(userMessage, prefix, out int argPos)) {
				RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessage, null);

				await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_ConfigService.MultiMatchHandling);
			}
		}
	}
}
