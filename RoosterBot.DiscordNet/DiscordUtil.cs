using Discord;

namespace RoosterBot.DiscordNet {
	internal class DiscordUtil {
		public static bool IsMessageCommand(Discord.IMessage message, string prefix, out int argPos) {
			argPos = 0;
			if (message.Content != null && // Message objects created for MessageUpdated events only contain what was modified. Content may be null in certain cases. https://github.com/discord-net/Discord.Net/issues/1409
				message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > prefix.Length &&
				userMessage.Content.StartsWith(prefix)) {
				// First char after prefix
				char firstChar = message.Content[prefix.Length];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}
	}
}
