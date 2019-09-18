using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RoosterBot {
	// TODO all responses should be deleted when the command is deleted, even if the module does not derive from EditableCmdModuleBase
	internal sealed class DeletedCommandHandler {
		private readonly RoosterCommandService m_Commands;

		public DeletedCommandHandler(DiscordSocketClient client, RoosterCommandService commands) {
			m_Commands = commands;

			client.MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (m_Commands.RemoveCommand(message.Id, out CommandResponsePair crp)) {
				await Util.DeleteAll(channel, crp.Responses);
			}
		}
	}
}
