using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageUpdatedHandler {
		public ChannelConfigService CCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;

		internal MessageUpdatedHandler(IServiceProvider isp) {
			CCS = isp.GetRequiredService<ChannelConfigService>();
			UCS = isp.GetRequiredService<UserConfigService>();
			DiscordNetComponent.Instance.Client.MessageUpdated += OnMessageUpdated;
		}

		private Task OnMessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageBefore.HasValue &&
				messageAfter.Content != null &&
				messageAfter.Content != messageBefore.Value.Content &&
				messageAfter is SocketUserMessage userMessageAfter &&
				messageAfter.Source == Discord.MessageSource.User) {
				_ = Task.Run(async () => {
					try {
						ChannelConfig guildConfig = await CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance,
							((channel as Discord.IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First()).Id));

						UserConfig userConfig = await UCS.GetConfigAsync(new DiscordUser(messageAfter.Author).GetReference());
						CommandResponsePair? crp = userConfig.GetResponse(new SnowflakeReference(DiscordNetComponent.Instance, userMessageAfter));

						if (DiscordUtil.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
							await Program.Instance.CommandHandler.ExecuteCommandAsync(userMessageAfter.Content.Substring(argPos), new DiscordCommandContext(new DiscordMessage(userMessageAfter), userConfig, guildConfig));
						} else if (crp != null) {
							// No longer a command
							await channel.DeleteMessageAsync((ulong) crp.Response.Id);
							userConfig.RemoveCommand(crp.Command);
						} // else: was not a command, is not a command
					} catch (Exception e) {
						Logger.Error("Discord", "Exception caught when handling edited message", e);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
