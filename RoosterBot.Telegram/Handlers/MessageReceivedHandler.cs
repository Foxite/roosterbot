using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	internal sealed class MessageReceivedHandler {
		public UserConfigService UCS { get; set; } = null!;
		public ChannelConfigService CCS { get; set; } = null!;

		public MessageReceivedHandler(IServiceProvider isp) {
			CCS = isp.GetRequiredService<ChannelConfigService>();
			UCS = isp.GetRequiredService<UserConfigService>();
			TelegramComponent.Instance.Client.OnMessage += OnMessageReceived;
		}

		private void OnMessageReceived(object? sender, MessageEventArgs e) => Task.Run(async () => {
			if (e.Message.Type == MessageType.Text) {
				ChannelConfig channelConfig = await CCS.GetConfigAsync(new TelegramChannel(e.Message.Chat).GetReference());
				if (e.Message.Text.StartsWith(channelConfig.CommandPrefix)) {
					await TelegramComponent.Instance.Client.SendChatActionAsync(e.Message.Chat, ChatAction.Typing);
					await Program.Instance.CommandHandler.ExecuteCommandAsync(
						e.Message.Text.Substring(channelConfig.CommandPrefix.Length),
						new TelegramCommandContext(
							e.Message,
							await UCS.GetConfigAsync(new TelegramUser(e.Message.From).GetReference()),
							channelConfig
						)
					);
				}
			}
		});
	}
}