using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageReceivedHandler {
		public ChannelConfigService CCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;

		internal MessageReceivedHandler(IServiceProvider isp) {
			CCS = isp.GetRequiredService<ChannelConfigService>();
			UCS = isp.GetRequiredService<UserConfigService>();

			DiscordNetComponent.Instance.Client.MessageReceived += HandleNewCommand;
		}

		private Task HandleNewCommand(SocketMessage dsm) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (dsm is Discord.IUserMessage dum) {
				_ = Task.Run(async () => {
					try {
						// RoosterBot doesn't have a concept of guilds, and in Discord it's not convention to have different config per channel.
						// So we secretly use guilds instead of channels for channel config.
						ChannelConfig guildConfig = await CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, (dum.Channel is Discord.IGuildChannel igc) ? igc.GuildId : dum.Channel.Id));
						if (DiscordUtil.IsMessageCommand(dum, guildConfig.CommandPrefix, out int argPos)) {
							UserConfig userConfig = await UCS.GetConfigAsync(new DiscordUser(dum.Author).GetReference());
							await dum.Channel.TriggerTypingAsync();
							await Program.Instance.CommandHandler.ExecuteCommandAsync(dum.Content.Substring(argPos), new DiscordCommandContext(new DiscordMessage(dum), userConfig, guildConfig));
						}
					} catch (Exception e) {
						Logger.Error("Discord", "Exception caught when handling new message", e);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
