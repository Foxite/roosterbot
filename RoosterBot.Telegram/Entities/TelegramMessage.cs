using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RoosterBot.Telegram {
	public class TelegramMessage : TelegramSnowflake, IMessage {
		public Message TelegramEntity { get; }

		public override long Id => TelegramEntity.MessageId;
		public IChannel Channel => new TelegramChannel(TelegramEntity.Chat);
		public IUser User => new TelegramUser(TelegramEntity.From);
		public bool SentByRoosterBot => TelegramEntity.From.Id == TelegramComponent.Instance.Client.BotId;
		public string Content => TelegramEntity.Text;
		public Task DeleteAsync() => TelegramComponent.Instance.Client.DeleteMessageAsync(TelegramEntity.Chat, TelegramEntity.MessageId);
		public Task ModifyAsync(string newContent, string? filePath = null) => TelegramComponent.Instance.Client.EditMessageTextAsync(TelegramEntity.Chat, TelegramEntity.MessageId, newContent);

		public TelegramMessage(Message telegramEntity) {
			TelegramEntity = telegramEntity;
		}
	}
}
