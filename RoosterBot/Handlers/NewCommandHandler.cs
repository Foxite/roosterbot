using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class NewCommandHandler {
		private RoosterCommandService m_Commands;
		private ConfigService m_ConfigService;
		private DiscordSocketClient m_Client;

		internal NewCommandHandler(DiscordSocketClient client, RoosterCommandService commands, ConfigService configService) {
			m_Client = client;
			m_Commands = commands;
			m_ConfigService = configService;

			m_Client.MessageReceived += HandleNewCommand;
		}

		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (m_Commands.IsMessageCommand(socketMessage, out int argPos)) {
				RoosterCommandContext context = new RoosterCommandContext(m_Client, socketMessage as IUserMessage, null, "NewCommand");

				await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services, m_ConfigService.MultiMatchHandling);
			}
		}
	}
}
