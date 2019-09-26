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
				RoosterCommandService.CommandResponsePair crp = m_Commands.GetResponse(userMessageAfter);
				if (m_Commands.IsMessageCommand(userMessageAfter, out int argPos)) {
					IUserMessage[] responses;
					if (crp != null) {
						// Was previously a command
						responses = crp.Responses;
					} else {
						// Was previously not a command
						responses = null;
					}

					RoosterCommandContext context = new RoosterCommandContext(m_Client, userMessageAfter, responses, "EditedCommand");
					await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_Config.MultiMatchHandling);
				} else {
					if (crp != null) {
						// No longer a command
						m_Commands.RemoveCommand(userMessageAfter, out _);
						await Util.DeleteAll(crp.Command.Channel, crp.Responses);
					} // else: was not a command, is not a command
				}
			}
		}
	}
}
