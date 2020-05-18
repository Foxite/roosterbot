using Discord;

namespace RoosterBot.DiscordNet {
	internal class DiscordUtil {
		public static bool IsMessageCommand(Discord.IMessage message, string prefix, out int argPos) {
			argPos = 0;
			if (message.Content != null && // Message objects created for MessageUpdated events only contain what was modified. Content may be null in certain cases. https://github.com/discord-net/Discord.Net/issues/1409
				message is IUserMessage userMessage) {
				string mention = DiscordNetComponent.Instance.Client.CurrentUser.Mention;
				if (userMessage.Content.StartsWith(mention)) {
					prefix = mention;
				} else if (!userMessage.Content.StartsWith(prefix)) {
					return false;
				}
				if (userMessage.Content.Length == prefix.Length) {
					return false;
				}

				argPos = prefix.Length;

				while (userMessage.Content.Length > argPos && userMessage.Content[argPos] == ' ') {
					// Skip whitespace
					argPos++;
				}

				// First char after prefix
				char firstChar = message.Content[argPos];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Otherwise probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					// TODO this means commands that start with a non alphabetic character don't work on Discord.
					// I don't want to ignore unknown commands like Dyno does, but I also don't want to respond to !!! etc.

					return true;
				}
			}

			return false;
		}
	}
}
