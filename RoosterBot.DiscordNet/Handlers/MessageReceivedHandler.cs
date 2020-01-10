using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class MessageReceivedHandler : RoosterHandler {
		public ChannelConfigService CCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;
		public BaseSocketClient Client { get; set; } = null!;

		internal MessageReceivedHandler(IServiceProvider isp) : base(isp) {
			Client.MessageReceived += HandleNewCommand;
		}

		private Task HandleNewCommand(SocketMessage dsm) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (dsm is Discord.IUserMessage dum) {
				_ = Task.Run(async () => {
					// RoosterBot doesn't have a concept of guilds, and in Discord it's not convention to have different config per channel.
					// So we secretly use guilds instead of channels for channel config.
					ChannelConfig guildConfig = await CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, (dum.Channel is Discord.IGuildChannel igc) ? igc.GuildId : dum.Channel.Id));
					if (DiscordUtil.IsMessageCommand(dum, guildConfig.CommandPrefix, out int argPos)) {
						UserConfig userConfig = await UCS.GetConfigAsync(new DiscordUser(dum.Author).GetReference());
						await Program.Instance.ExecuteHandler.ExecuteCommandAsync(dum.Content.Substring(argPos + 1), new DiscordCommandContext(new DiscordMessage(dum), userConfig, guildConfig));
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
