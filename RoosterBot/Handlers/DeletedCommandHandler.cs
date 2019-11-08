using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RoosterBot {
	internal sealed class DeletedCommandHandler {
		private readonly CommandResponseService m_CRS;

		public DeletedCommandHandler(DiscordSocketClient client, CommandResponseService crs) {
			m_CRS = crs;

			client.MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (m_CRS.RemoveCommand(message.Id, out CommandResponsePair? crp)) {
				await Util.DeleteAll(channel, crp.Responses);
			}
		}
	}
}
