using System.Linq;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// A static class containing some helper functions for dealing with commands.
	/// </summary>
	public static class CommandUtil {
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
