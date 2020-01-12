using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RoosterBot.Telegram {
	public class TelegramUser : TelegramSnowflake, IUser {
		public User TelegramEntity { get; }

		public int Id => TelegramEntity.Id;

		public string UserName => TelegramEntity.Username;
		public string DisplayName => (TelegramEntity.FirstName + " " + TelegramEntity.LastName).Trim();
		public string Mention => "@" + TelegramEntity.Username;
		public bool IsBotAdmin => TelegramComponent.Instance.BotOwnerId == TelegramEntity.Id;

		object ISnowflake.Id => Id;

		public TelegramUser(User telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		public bool IsChannelAdmin(IChannel ic) => false; // TODO
		public Task<IChannel?> GetPrivateChannel() => throw new NotImplementedException(); // TODO
	}
}
