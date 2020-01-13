using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	internal sealed class MessageEditedHandler : RoosterHandler {
		public UserConfigService UCS { get; set; } = null!;
		public ChannelConfigService CCS { get; set; } = null!;

		public MessageEditedHandler(IServiceProvider isp) : base(isp) {
			TelegramComponent.Instance.Client.OnMessageEdited += OnMessageEdited;
		}

		private void OnMessageEdited(object? sender, MessageEventArgs e) => Task.Run(async () => {
			if (e.Message.Type == MessageType.Text) {
				ChannelConfig channelConfig = await CCS.GetConfigAsync(new TelegramChannel(e.Message.Chat).GetReference());
				UserConfig userConfig = await UCS.GetConfigAsync(new TelegramUser(e.Message.From).GetReference());
				if (e.Message.Text.StartsWith(channelConfig.CommandPrefix)) {
					await Program.Instance.CommandHandler.ExecuteCommandAsync(
						e.Message.Text.Substring(channelConfig.CommandPrefix.Length),
						new TelegramCommandContext(
							e.Message,
							userConfig,
							channelConfig
						)
					);
				} else {
					CommandResponsePair? crp = userConfig.GetResponse(new TelegramMessage(e.Message));
					if (crp != null) {
						await TelegramComponent.Instance.Client.DeleteMessageAsync(e.Message.Chat, (int) crp.Response.Id);
					}
				}
			}
		});
	}
}