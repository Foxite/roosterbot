using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageDeletedHandler {
		public UserConfigService UCS { get; set; } = null!;

		public MessageDeletedHandler(IServiceProvider isp) {
			UCS = isp.GetRequiredService<UserConfigService>();
			isp.GetRequiredService<BaseSocketClient>().MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> message, Discord.IMessageChannel channel) {
			if (message.HasValue && message.Value is Discord.IUserMessage userMessage) {
				UserConfig userConfig = await UCS.GetConfigAsync(new DiscordUser(message.Value.Author).GetReference());
				CommandResponsePair? crp = userConfig.GetResponse(new SnowflakeReference(DiscordNetComponent.Instance, userMessage.Id));
				if (crp != null) {
					await channel.DeleteMessageAsync((ulong) crp.Response.Id);
					userConfig.RemoveCommand(crp.Command);
				}
			}
		}
	}
}
