using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class EditedCommandHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly ConfigService m_Config;

		internal EditedCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService config) {
			m_Client = client;
			m_Commands = commands;
			m_Config = config;

			m_Client.MessageUpdated += OnMessageUpdated;
		}

		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter is SocketUserMessage userMessageAfter) {
				CommandResponsePair crp = m_Commands.GetResponse(userMessageAfter);
				if (crp != null) {
					crp.Command = userMessageAfter;

					if (m_Commands.IsMessageCommand(crp.Command, out int argPos)) {
						RoosterCommandContext context = new RoosterCommandContext(m_Client, crp.Command, crp.Responses, "EditedCommand");

						await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_Config.MultiMatchHandling);
					} else {
						await Util.DeleteAll(crp.Command.Channel, crp.Responses);
					}
				}
			}
		}
	}
}
