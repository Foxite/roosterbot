using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageUpdatedHandler {
		private readonly ChannelConfigService m_CCS;
		private readonly UserConfigService m_UCS;
		private readonly IServiceProvider m_ISP;

		internal MessageUpdatedHandler(IServiceProvider isp) {
			m_CCS = isp.GetRequiredService<ChannelConfigService>();
			m_UCS = isp.GetRequiredService<UserConfigService>();
			m_ISP = isp;

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
						ChannelConfig guildConfig = await m_CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, ((channel as Discord.IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First()).Id));

						UserConfig userConfig = await m_UCS.GetConfigAsync(new DiscordUser(messageAfter.Author).GetReference());
						CommandResponsePair? crp = userConfig.GetResponse(new SnowflakeReference(DiscordNetComponent.Instance, userMessageAfter.Id));

						if (DiscordUtil.IsMessageCommand(userMessageAfter.Content, guildConfig.CommandPrefix, out int argPos)) {
							await Program.Instance.CommandHandler.ExecuteCommandAsync(userMessageAfter.Content[argPos..], new DiscordCommandContext(m_ISP, userMessageAfter, userConfig, guildConfig));
						} else if (crp != null) {
							// No longer a command
							await channel.DeleteMessageAsync((ulong) crp.Response.Id);
							userConfig.RemoveCommand(crp.Command);
						} // else: was not a command, is not a command
					} catch (Exception e) {
						Logger.Error(DiscordNetComponent.LogTag, "Exception caught when handling edited message", e);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
