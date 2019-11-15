using Discord;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoosterBot {
	public static class DiscordUtil {
		public static async Task<bool> AddReaction(IUserMessage message, string unicode) {
			try {
				await message.AddReactionAsync(new Emoji(unicode));
				return true;
			} catch (HttpException e) { // Permission denied
				Logger.Warning("Util", "Attempted to add a reaction to a message, but this failed", e);
				return false;
			}
		}

		public static async Task<bool> RemoveReaction(IUserMessage message, string unicode, IUser botUser) {
			try {
				await message.RemoveReactionAsync(new Emoji(unicode), botUser);
				return true;
			} catch (HttpException e) { // Permission denied
				Logger.Warning("Util", "Attempted to add a reaction to a message, but this failed", e);
				return false;
			}
		}

		public static async Task DeleteAll(IMessageChannel channel, IEnumerable<IUserMessage> messages) {
			if (channel is ITextChannel textChannel) {
				await textChannel.DeleteMessagesAsync(messages);
			} else {
				// No idea what kind of non-text MessageChannel there are, but at least they support non-bulk deletion.
				foreach (IUserMessage message in messages) {
					await message.DeleteAsync();
				}
			}
		}

		/// <summary>
		/// Given a set of messages that we sent, this will delete all messages except the first, and modify the first according to <paramref name="message"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="responses"></param>
		/// <param name="append">If true, this will append <paramref name="message"/> to the first response, otherwise this will overwrite the contents of that message.</param>
		/// <returns></returns>
		public static async Task<IUserMessage> ModifyResponsesIntoSingle(string message, IEnumerable<IUserMessage> responses, bool append) {
			IUserMessage singleResponse = responses.First();
			IEnumerable<IUserMessage> extraMessages = responses.Skip(1);

			if (extraMessages.Any()) {
				await DeleteAll(singleResponse.Channel, extraMessages);
			}

			await singleResponse.ModifyAsync((msgProps) => {
				if (append) {
					msgProps.Content += "\n\n" + message;
				} else {
					msgProps.Content = message;
				}
			});
			return singleResponse;
		}
	}
}
