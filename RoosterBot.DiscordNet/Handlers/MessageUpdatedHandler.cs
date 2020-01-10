using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageUpdatedHandler : RoosterHandler {
		public BaseSocketClient Client { get; set; } = null!;
		public ChannelConfigService CCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;

		internal MessageUpdatedHandler(IServiceProvider isp) : base(isp) {
			Client.MessageUpdated += OnMessageUpdated;
		}

		private Task OnMessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageBefore.HasValue &&
				messageAfter.Content != null && 
				messageAfter.Content != messageBefore.Value.Content &&
				messageAfter is SocketUserMessage userMessageAfter &&
				messageAfter.Source == Discord.MessageSource.User) {
				_ = Task.Run(async () => {
					ChannelConfig guildConfig = await CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance,
						((channel as Discord.IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First()).Id));

					UserConfig userConfig = await UCS.GetConfigAsync(new DiscordUser(messageAfter.Author).GetReference());
					CommandResponsePair? crp = userConfig.GetResponse(userMessageAfter);

					if (DiscordUtil.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
						await Program.Instance.ExecuteHandler.ExecuteCommandAsync(DiscordNetComponent.Instance, userMessageAfter.Content.Substring(argPos + 1), new DiscordMessage(userMessageAfter), guildConfig, userConfig);
					} else if (crp != null) {
						// No longer a command
						await channel.DeleteMessageAsync((ulong) crp.ResponseId);
						userConfig.RemoveCommand(crp.CommandId);
					} // else: was not a command, is not a command
				});
			}
			return Task.CompletedTask;
		}
	}
}
