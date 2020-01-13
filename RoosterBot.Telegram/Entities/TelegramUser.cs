using System.Linq;
using Telegram.Bot.Types;

namespace RoosterBot.Telegram {
	public class TelegramUser : TelegramSnowflake, IUser {
		public User TelegramEntity { get; }

		public override long Id => TelegramEntity.Id;

		public string UserName => TelegramEntity.Username;
		public string DisplayName => (TelegramEntity.FirstName + " " + TelegramEntity.LastName).Trim();
		public string Mention => "@" + TelegramEntity.Username;
		public bool IsBotAdmin => TelegramComponent.Instance.BotOwnerId == TelegramEntity.Id;

		public TelegramUser(User telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		public bool IsChannelAdmin(IChannel ic) {
			if (ic is TelegramChannel channel) {
				return TelegramComponent.Instance.Client.GetChatAdministratorsAsync(channel.TelegramEntity).Result.Any(member => member.User == TelegramEntity);
			} else {
				return false;
			}
		}
	}
}
