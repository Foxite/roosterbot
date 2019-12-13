using System.Linq;
using Discord;
using Qmmands;

namespace RoosterBot {
	public static class CommandUtil {
		public static bool IsMessageCommand(IMessage message, string prefix, out int argPos) {
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

		/// <summary>
		/// Returns a localized signature of a CommandInfo.
		/// </summary>
		public static string GetSignature(this Command command) {
			string ret = command.FullAliases.First() + " ";

			foreach (Parameter param in command.Parameters) {
				if (param.Name != "ignored") {
					string paramText = param.Name;

					TypeDisplayAttribute? typeDisplayAttr = param.Attributes.OfType<TypeDisplayAttribute>().SingleOrDefault();
					if (typeDisplayAttr != null) {
						paramText += ": " + typeDisplayAttr.Text;
					}

					if (param.IsMultiple) {
						paramText += "...";
					}

					if (!param.Attributes.OfType<GrammarParameterAttribute>().Any()) {
						if (param.IsOptional) {
							paramText = "[" + paramText + "]";
						} else {
							paramText = "<" + paramText + ">";
						}
					}
					ret += paramText + " ";
				}
			}
			return ret.Trim();
		}
	}
}
