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
		private readonly ConfigService m_Config;
		private readonly GuildConfigService m_GCS;
		private readonly UserConfigService m_UCS;

		internal EditedCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService config, GuildConfigService gcs, UserConfigService ucs) {
			m_Client = client;
			m_Commands = commands;
			m_Config = config;
			m_GCS = gcs;
			m_UCS = ucs;
			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter is SocketUserMessage userMessageAfter && messageAfter.Source == MessageSource.User) {
				GuildConfig guildConfig = await m_GCS.GetConfigAsync((channel as IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First());
				UserConfig userConfig = await m_UCS.GetConfigAsync(messageAfter.Author);
				CommandResponsePair? crp = userConfig.GetResponse(userMessageAfter);

				if (m_Commands.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
					IUserMessage? response = crp == null ? null : (await channel.GetMessageAsync(crp.ResponseId)) as IUserMessage;

					RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessageAfter, response, userConfig, guildConfig);
					await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_Config.MultiMatchHandling);
				} else if (crp != null) {
					// No longer a command
					await userConfig.RemoveCommandAsync(crp.CommandId);
					await channel.DeleteMessageAsync(crp.ResponseId);
				} // else: was not a command, is not a command
			}
		}
	}
}
