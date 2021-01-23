using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RoosterBot.Telegram {
	public class TelegramCommandContext : RoosterCommandContext {
		public new Message Message { get; }
		public new Chat Channel { get; }
		public new User User { get; }

		public TelegramCommandContext(IServiceProvider isp, Message message, UserConfig userConfig, ChannelConfig channelConfig)
			: base(isp, TelegramComponent.Instance, new TelegramMessage(message), userConfig, channelConfig) {
			Message = message;
			Channel = message.Chat;
			User = message.From;
		}

		// TODO Use adapters
		protected override Task<IMessage> SendResultAsync(RoosterCommandResult result) => throw new NotImplementedException();
	}
}
