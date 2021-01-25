using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace RoosterBot.Telegram {
	public class TelegramMessage : TelegramSnowflake, IMessage {
		public Message TelegramEntity { get; }

		public override long Id => TelegramEntity.MessageId;
		public IChannel Channel => new TelegramChannel(TelegramEntity.Chat);
		public IUser User => new TelegramUser(TelegramEntity.From);
		public string Content => TelegramEntity.Text;

		object ISnowflake.Id => Id;
		public DateTimeOffset SentAt => TelegramEntity.EditDate ?? TelegramEntity.ForwardDate ?? TelegramEntity.Date;

		public TelegramMessage(Message telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		public Task DeleteAsync() => TelegramComponent.Instance.Client.DeleteMessageAsync(TelegramEntity.Chat, TelegramEntity.MessageId);
	}
}
