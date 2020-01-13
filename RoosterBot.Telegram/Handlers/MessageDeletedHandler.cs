using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	internal sealed class MessageDeletedHandler : RoosterHandler {
		public UserConfigService UCS { get; set; } = null!;
		public ChannelConfigService CCS { get; set; } = null!;

		public MessageDeletedHandler(IServiceProvider isp) : base(isp) {
			// TODO execute when a message is deleted
			//TelegramComponent.Instance.Client.OnMessageDeleted += OnMessageDeleted;
		}

		private void OnMessageDeleted(object? sender, MessageEventArgs e) => Task.Run(async () => {
			if (e.Message.Type == MessageType.Text) {
				
				UserConfig userConfig = await UCS.GetConfigAsync(new TelegramUser(e.Message.From).GetReference());
				CommandResponsePair? crp = userConfig.GetResponse(new TelegramMessage(e.Message));
				if (crp != null) {
					await TelegramComponent.Instance.Client.DeleteMessageAsync(e.Message.Chat, (int) crp.Response.Id);
				}
			}
		});
	}
}