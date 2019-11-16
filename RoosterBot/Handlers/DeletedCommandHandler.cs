using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class DeletedCommandHandler {
		private readonly UserConfigService m_UCS;

		public DeletedCommandHandler(DiscordSocketClient client, UserConfigService ucs) {

			client.MessageDeleted += OnMessageDeleted;
			m_UCS = ucs;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (message.HasValue && message.Value is IUserMessage userMessage) {
				UserConfig userConfig = await m_UCS.GetConfigAsync(message.Value.Author);
				CommandResponsePair? crp = userConfig.GetResponse(userMessage);
				if (crp != null) {
					await Task.WhenAll(new[] {
						channel.DeleteMessageAsync(crp.ResponseId),
						userConfig.RemoveCommandAsync(crp.CommandId)
					});
				}
			}
		}
	}
}
