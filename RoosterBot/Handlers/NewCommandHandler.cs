using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class NewCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly ConfigService m_ConfigService;
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;

		internal NewCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService configService, GuildConfigService gcs, UserConfigService ucs) {
			m_Client = client;
			m_Commands = commands;
			m_ConfigService = configService;
			m_GCS = gcs;
			m_UCS = ucs;
			m_Client.MessageReceived += HandleNewCommand;
		}

		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (socketMessage is IUserMessage userMessage) {
				GuildConfig guildConfig = await m_GCS.GetConfigAsync((socketMessage.Channel as IGuildChannel)?.Guild ?? socketMessage.Author.MutualGuilds.First());
				if (m_Commands.IsMessageCommand(userMessage, guildConfig.CommandPrefix, out int argPos)) {
					UserConfig userConfig = await m_UCS.GetConfigAsync(userMessage.Author);

					RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessage, null, userConfig, guildConfig);

					await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_ConfigService.MultiMatchHandling);
				}
			}
		}
	}
}
