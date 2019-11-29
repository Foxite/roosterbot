using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class MessageUpdatedHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;
		private readonly CommandExecutionHandler m_CEH;

		internal MessageUpdatedHandler(DiscordSocketClient client, RoosterCommandService commands, GuildConfigService gcs, UserConfigService ucs, CommandExecutionHandler ceh) {
			m_Client = client;
			m_GCS = gcs;
			m_UCS = ucs;
			m_CEH = ceh;
			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter.Content != messageBefore.Value.Content && messageAfter is SocketUserMessage userMessageAfter && messageAfter.Source == MessageSource.User) {
				_ = Task.Run(async () => {
					GuildConfig guildConfig = await m_GCS.GetConfigAsync((channel as IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First());
					UserConfig userConfig = await m_UCS.GetConfigAsync(messageAfter.Author);
					CommandResponsePair? crp = userConfig.GetResponse(userMessageAfter);

					if (CommandUtil.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
						//IResult result = await m_Commands.ExecuteAsync(userMessageAfter.Content.Substring(argPos + 1), context);
						await m_CEH.ExecuteCommandAsync(userMessageAfter.Content.Substring(argPos + 1), userMessageAfter, guildConfig, userConfig);
					} else if (crp != null) {
						// No longer a command
						await channel.DeleteMessageAsync(crp.ResponseId);
						userConfig.RemoveCommand(crp.CommandId);
					} // else: was not a command, is not a command
				});
			}
			return Task.CompletedTask;
		}
	}
}
