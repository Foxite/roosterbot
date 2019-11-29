using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace RoosterBot {
	internal sealed class NewCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;
		private readonly PostCommandHandler m_SPCH;

		internal NewCommandHandler(DiscordSocketClient client, RoosterCommandService commands, GuildConfigService gcs, UserConfigService ucs, PostCommandHandler spch) {
			m_Client = client;
			m_Commands = commands;
			m_GCS = gcs;
			m_UCS = ucs;
			m_SPCH = spch;
			m_Client.MessageReceived += HandleNewCommand;
		}

		private Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (socketMessage is IUserMessage userMessage) {
				_ = Task.Run(async () => {
					GuildConfig guildConfig = await m_GCS.GetConfigAsync((socketMessage.Channel as IGuildChannel)?.Guild ?? socketMessage.Author.MutualGuilds.First());
					if (CommandUtil.IsMessageCommand(userMessage, guildConfig.CommandPrefix, out int argPos)) {
						UserConfig userConfig = await m_UCS.GetConfigAsync(userMessage.Author);

						var context = new RoosterCommandContext(m_Client, userMessage, userConfig, guildConfig, Program.Instance.Components.Services);

						IResult result = await m_Commands.ExecuteAsync(userMessage.Content.Substring(argPos + 1), context);
						await m_SPCH.HandleResultAsync(result, context);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
