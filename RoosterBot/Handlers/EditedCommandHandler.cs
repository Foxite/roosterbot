using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class EditedCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly CommandResponseService m_CRS;
		private readonly ConfigService m_Config;
		private readonly GuildConfigService m_GCS;

		internal EditedCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService config, CommandResponseService crs, GuildConfigService gcs) {
			m_Client = client;
			m_Commands = commands;
			m_Config = config;
			m_CRS = crs;
			m_GCS = gcs;

			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter is SocketUserMessage userMessageAfter) {
				CommandResponsePair crp = m_CRS.GetResponse(userMessageAfter);
				string prefix;
				if (channel is IGuildChannel guildChannel) {
					prefix = (await m_GCS.GetConfigAsync(guildChannel.Guild)).CommandPrefix;
				} else {
					prefix = m_Config.DefaultCommandPrefix;
				}

				if (m_Commands.IsMessageCommand(userMessageAfter, prefix, out int argPos)) {
					IReadOnlyCollection<IUserMessage> responses;
					if (crp != null) {
						// Was previously a command
						responses = crp.Responses;
					} else {
						// Was previously not a command
						responses = null;
					}

					RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessageAfter, responses);
					await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_Config.MultiMatchHandling);
				} else {
					if (crp != null) {
						// No longer a command
						m_CRS.RemoveCommand(userMessageAfter, out _);
						await Util.DeleteAll(crp.Command.Channel, crp.Responses);
					} // else: was not a command, is not a command
				}
			}
		}
	}
}
