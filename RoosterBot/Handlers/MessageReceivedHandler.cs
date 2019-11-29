using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class MessageReceivedHandler {
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;
		private readonly CommandExecutionHandler m_CEH;

		internal MessageReceivedHandler(DiscordSocketClient client, GuildConfigService gcs, UserConfigService ucs, CommandExecutionHandler ceh) {
			m_GCS = gcs;
			m_UCS = ucs;
			m_CEH = ceh;

			client.MessageReceived += HandleNewCommand;
		}

		private Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (socketMessage is IUserMessage userMessage) {
				_ = Task.Run(async () => {
					GuildConfig guildConfig = await m_GCS.GetConfigAsync((socketMessage.Channel as IGuildChannel)?.Guild ?? socketMessage.Author.MutualGuilds.First());
					if (CommandUtil.IsMessageCommand(userMessage, guildConfig.CommandPrefix, out int argPos)) {
						UserConfig userConfig = await m_UCS.GetConfigAsync(userMessage.Author);
						await m_CEH.ExecuteCommandAsync(userMessage.Content.Substring(argPos + 1), userMessage, guildConfig, userConfig);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
