namespace RoosterBot.DiscordNet {
	internal class DiscordUtil {
		public static bool IsMessageCommand(string content, string prefix, out int argPos) {
			argPos = 0;
			string mention = DiscordNetComponent.Instance.Client.CurrentUser.Mention;
			if (content.StartsWith(mention)) {
				prefix = mention;
			} else if (!content.StartsWith(prefix)) {
				return false;
			}
			if (content.Length == prefix.Length) {
				return false;
			}

			argPos = prefix.Length;

			while (content.Length > argPos && content[argPos] == ' ') {
				// Skip whitespace
				argPos++;
			}

			// First char after prefix
			char firstChar = content[argPos];
			if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
				// Otherwise probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
				// TODO (fix) this means commands that start with a non alphabetic character don't work on Discord.
				// I don't want to ignore unknown commands like Dyno does, but I also don't want to respond to !!! etc.

				return true;
			}
			return false;
		}
	}
}
