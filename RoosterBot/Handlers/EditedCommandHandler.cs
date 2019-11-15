using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly UserConfigService m_UCS;

		internal EditedCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService config, CommandResponseService crs, GuildConfigService gcs, UserConfigService ucs) {
			m_Client = client;
			m_Commands = commands;
			m_Config = config;
			m_CRS = crs;
			m_GCS = gcs;
			m_UCS = ucs;
			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter is SocketUserMessage userMessageAfter) {
				CommandResponsePair? crp = m_CRS.GetResponse(userMessageAfter);
				GuildConfig guildConfig = await m_GCS.GetConfigAsync((channel as IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First());

				if (m_Commands.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
					IReadOnlyCollection<IUserMessage>? responses = null;
					if (crp != null) {
						// Was previously a command
						responses = crp.Responses;
					} // else: Was previously not a command. Use default value
					UserConfig userConfig = await m_UCS.GetConfigAsync(messageAfter.Author);

					RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessageAfter, responses, userConfig, guildConfig);
					await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_Config.MultiMatchHandling);
				} else if (crp != null) {
					// No longer a command
					m_CRS.RemoveCommand(userMessageAfter, out _);
					await Util.DeleteAll(crp.Command.Channel, crp.Responses);
				} // else: was not a command, is not a command
			}
		}
	}
}
