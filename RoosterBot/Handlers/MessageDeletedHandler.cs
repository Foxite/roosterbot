using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	internal sealed class MessageDeletedHandler : RoosterHandler {
		public UserConfigService UCS { get; set; } = null!;

		public MessageDeletedHandler(IServiceProvider isp) : base(isp) {
			isp.GetService<DiscordSocketClient>().MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (message.HasValue && message.Value is IUserMessage userMessage) {
				UserConfig userConfig = await UCS.GetConfigAsync(message.Value.Author);
				CommandResponsePair? crp = userConfig.GetResponse(userMessage);
				if (crp != null) {
					await channel.DeleteMessageAsync(crp.ResponseId);
					userConfig.RemoveCommand(crp.CommandId);
				}
			}
		}
	}
}
