using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	internal sealed class MessageReceivedHandler : RoosterHandler {
		public UserConfigService UCS { get; set; } = null!;
		public ChannelConfigService CCS { get; set; } = null!;

		public MessageReceivedHandler(IServiceProvider isp) : base(isp) {
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