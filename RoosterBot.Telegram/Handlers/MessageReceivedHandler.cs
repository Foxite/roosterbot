using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	internal sealed class MessageReceivedHandler {
		private readonly UserConfigService m_UCS;
		private readonly ChannelConfigService m_CCS;
		private readonly IServiceProvider m_ISP;

		public MessageReceivedHandler(IServiceProvider isp) {
			m_CCS = isp.GetRequiredService<ChannelConfigService>();
			m_UCS = isp.GetRequiredService<UserConfigService>();
			m_ISP = isp;

			TelegramComponent.Instance.Client.OnMessage += OnMessageReceived;
		}

		private void OnMessageReceived(object? sender, MessageEventArgs e) => Task.Run(async () => {
			if (e.Message.Type == MessageType.Text) {
				ChannelConfig channelConfig = await m_CCS.GetConfigAsync(new TelegramChannel(e.Message.Chat).GetReference());
				if (e.Message.Text.StartsWith(channelConfig.CommandPrefix)) {
					await TelegramComponent.Instance.Client.SendChatActionAsync(e.Message.Chat, ChatAction.Typing);
					await Program.Instance.CommandHandler.ExecuteCommandAsync(
						e.Message.Text.Substring(channelConfig.CommandPrefix.Length),
						new TelegramCommandContext(
							m_ISP,
							e.Message,
							await m_UCS.GetConfigAsync(new TelegramUser(e.Message.From).GetReference()),
							channelConfig
						)
					);
				}
			}
		});
	}
}