using System;
using System.Globalization;
using System.Linq;
using Qmmands;

namespace RoosterBot {
	public static class CommandUtil {
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
