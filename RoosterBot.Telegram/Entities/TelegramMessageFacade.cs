using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace RoosterBot.Telegram {
	/// <summary>
	/// A "Fake" Telegram message that contains a ChatId and message ID, and does not represent a "real" <see cref="Message"/> object.
	/// Unfortunately it does not seem to be possible to obtain an actual Message object from an ID with Telegram.Bot without doing something to it, or when it is sent or received.
	/// 
	/// It supports deletion and modification, and retrieving the IChannel it belongs to.
	/// This is enough for what RoosterBot needs <see cref="IChannel.GetMessageAsync(object)"/> for, but keep that in mind when dealing with TelegramMessages.
	/// </summary>
	public class TelegramMessageFacade : TelegramSnowflake, IMessage {
		private const string NotImplementedMessage = @"TelegramMessageFacade is a ""Fake"" Telegram message that contains a ChatId and message ID, and does not represent a ""real"" Telegram.Bot.Types.Message object.
Unfortunately it does not seem to be possible to obtain an actual Message object from an ID with Telegram.Bot, you can only get these when a message is sent or received.";

		public override long Id { get; }

		public IChannel Channel { get; }
		public ChatId ChatId { get; }

		public IUser User => throw new NotImplementedException(NotImplementedMessage);
		public string Content => throw new NotImplementedException(NotImplementedMessage);

		public DateTimeOffset SentAt => throw new NotImplementedException(NotImplementedMessage);

		public TelegramMessageFacade(TelegramChannel channel, long id) {
			Id = id;
			Channel = channel;
			ChatId  = channel.Id;
		}

		public Task DeleteAsync() => TelegramComponent.Instance.Client.DeleteMessageAsync(ChatId, (int) Id);
		public Task ModifyAsync(string newContent, string? filePath = null) {
			if (filePath == null) {
				return TelegramComponent.Instance.Client.EditMessageTextAsync(ChatId, (int) Id, newContent, global::Telegram.Bot.Types.Enums.ParseMode.Markdown);
			} else {
				TelegramComponent.Instance.Client.EditMessageTextAsync(ChatId, (int) Id, newContent, global::Telegram.Bot.Types.Enums.ParseMode.Markdown);
				return TelegramComponent.Instance.Client.EditMessageMediaAsync(ChatId, (int) Id,
					new InputMediaDocument(new InputMedia(File.OpenRead(filePath), Path.GetFileName(filePath))));
			}
		}
	}
}
