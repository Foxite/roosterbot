using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RoosterBot.Telegram {
	public class TelegramChannel : TelegramSnowflake, IChannel {
		public Chat TelegramEntity { get; }

		public override long Id => TelegramEntity.Id;
		public string Name => TelegramEntity.Title;

		public TelegramChannel(Chat telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		public Task<IMessage> GetMessageAsync(object id) => throw new NotImplementedException(); // TODO
		public async Task<IMessage> SendMessageAsync(string content, string? filePath = null) => // TODO file uploads
			new TelegramMessage(await TelegramComponent.Instance.Client.SendTextMessageAsync(TelegramEntity, content));
	}
}
