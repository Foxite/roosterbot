using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace RoosterBot.Telegram {
	public class TelegramChannel : TelegramSnowflake, IChannel {
		public Chat TelegramEntity { get; }

		public override long Id => TelegramEntity.Id;
		public string Name => TelegramEntity.Title;

		object ISnowflake.Id => Id;

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
				throw new ArgumentException("ID must be long for Telegram entities.", nameof(id));
			}
		}

		public async Task<IMessage> SendMessageAsync(string content, string? filePath = null) {
			if (filePath == null) {
				return new TelegramMessage(await TelegramComponent.Instance.Client.SendTextMessageAsync(TelegramEntity, content, global::Telegram.Bot.Types.Enums.ParseMode.Markdown));
			} else {
				return new TelegramMessage(await TelegramComponent.Instance.Client.SendDocumentAsync(TelegramEntity,
					new InputOnlineFile(File.OpenRead(filePath), Path.GetFileName(filePath)), content, global::Telegram.Bot.Types.Enums.ParseMode.Markdown));
			}
		}
	}
}
