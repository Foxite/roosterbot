using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace RoosterBot.Telegram {
	public class TelegramChannel : TelegramSnowflake, IChannel {
		public Chat TelegramEntity { get; }

		public long Id => TelegramEntity.Id;
		public string Name => TelegramEntity.Title;

		object ISnowflake.Id => Id;

		public TelegramChannel(Chat telegramEntity) {
			TelegramEntity = telegramEntity;
		}

		public Task<IMessage> GetMessageAsync(object id) => throw new NotImplementedException(); // TODO

		public async Task<IMessage> SendMessageAsync(string content, string? filePath = null) {
			if (filePath == null) {
				return new TelegramMessage(await TelegramComponent.Instance.Client.SendTextMessageAsync(TelegramEntity, content));
			} else {
				return new TelegramMessage(await TelegramComponent.Instance.Client.SendDocumentAsync(TelegramEntity,
					new InputOnlineFile(File.OpenRead(filePath), Path.GetFileName(filePath)), content));
			}
		}
	}
}
