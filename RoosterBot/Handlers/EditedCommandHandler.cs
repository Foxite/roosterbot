using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace RoosterBot {
	internal sealed class EditedCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;
		private readonly SequentialPostCommandHandler m_SPCH;

		internal EditedCommandHandler(DiscordSocketClient client, RoosterCommandService commands, GuildConfigService gcs, UserConfigService ucs, SequentialPostCommandHandler spch) {
			m_Client = client;
			m_Commands = commands;
			m_GCS = gcs;
			m_UCS = ucs;
			m_SPCH = spch;

			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter.Content != messageBefore.Value.Content && messageAfter is SocketUserMessage userMessageAfter && messageAfter.Source == MessageSource.User) {
				GuildConfig guildConfig = await m_GCS.GetConfigAsync((channel as IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First());
				UserConfig userConfig = await m_UCS.GetConfigAsync(messageAfter.Author);
				CommandResponsePair? crp = userConfig.GetResponse(userMessageAfter);

				if (CommandUtil.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
					var context = new RoosterCommandContext(m_Client, userMessageAfter, userConfig, guildConfig, Program.Instance.Components.Services);
					IResult result = await m_Commands.ExecuteAsync(userMessageAfter.Content.Substring(argPos + 1), context);
					await m_SPCH.HandleResultAsync(result, context);
				} else if (crp != null) {
					// No longer a command
					await channel.DeleteMessageAsync(crp.ResponseId);
					userConfig.RemoveCommand(crp.CommandId);
				} // else: was not a command, is not a command
			}
		}
	}
}
