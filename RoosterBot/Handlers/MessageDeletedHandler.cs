using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class CommandDeletedHandler {
		private readonly UserConfigService m_UCS;

		public CommandDeletedHandler(DiscordSocketClient client, UserConfigService ucs) {
			m_UCS = ucs;
			client.MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (message.HasValue && message.Value is IUserMessage userMessage) {
				UserConfig userConfig = await m_UCS.GetConfigAsync(message.Value.Author);
				CommandResponsePair? crp = userConfig.GetResponse(userMessage);
				if (crp != null) {
					await channel.DeleteMessageAsync(crp.ResponseId);
					userConfig.RemoveCommand(crp.CommandId);
				}
			}
		}
	}
}
