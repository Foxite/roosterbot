using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RoosterBot.Telegram {
	public class TelegramChannel : TelegramSnowflake, IChannel {
		public Chat TelegramEntity { get; }

		public override long Id => TelegramEntity.Id;
		public string Name => TelegramEntity.Title;

		object ISnowflake.Id => Id;

		public bool IsPrivate => TelegramEntity.Type == ChatType.Private;

		public TelegramChannel(Chat telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		/// <summary>
		/// Returns a TelegramMessageFacade which <b>does not support</b> all operations. Refer to its documentation for details.
		/// </summary>
		public Task<IMessage> GetMessageAsync(object rawId) {
			if (rawId is long id) {
				return Task.FromResult<IMessage>(new TelegramMessageFacade(this, id));
			} else {
				throw new ArgumentException("ID must be long for Telegram entities.", nameof(rawId));
			}
		}

		public async Task<IMessage> SendMessageAsync(string content) {
			return new TelegramMessage(await TelegramComponent.Instance.Client.SendTextMessageAsync(TelegramEntity, content, ParseMode.Markdown));
		}
	}
}
