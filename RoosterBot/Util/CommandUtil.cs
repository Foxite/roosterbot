using System;
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
				char firstChar = message.Content.Substring(prefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}

		// TODO (localize) These nice names
		private static string GetNiceName(Type type) => type.Name switch {
			"Byte" => "integer",
			"SByte" => "integer",
			"Int16" => "integer",
			"Int32" => "integer",
			"Int64" => "integer",
			"UInt16" => "integer",
			"UInt32" => "integer",
			"UInt64" => "integer",
			"Single" => "decimal",
			"Double" => "decimal",
			"Decimal" => "decimal",
			"Char" => "character",
			"String" => "text",
			_ => type.Name,
		};

		/// <summary>
		/// Returns a localized signature of a CommandInfo.
		/// </summary>
		public static string GetSignature(this Command command) {
			string ret = command.FullAliases.First() + " ";

			foreach (Parameter param in command.Parameters) {
				if (param.Name != "ignored") {
					string paramText = param.Name + ": ";

					TypeDisplayAttribute? typeDisplayAttr = param.Attributes.OfType<TypeDisplayAttribute>().SingleOrDefault();
					paramText += typeDisplayAttr != null ? typeDisplayAttr.TypeDisplayName : GetNiceName(param.Type);

					if (param.IsMultiple) {
						paramText += "...";
					}

					if (param.IsOptional) {
						paramText = "[" + paramText + "] ";
					} else {
						paramText = "<" + paramText + "> ";
					}
					ret += paramText;
				}
			}
			return ret.Trim();
		}
	}
}
