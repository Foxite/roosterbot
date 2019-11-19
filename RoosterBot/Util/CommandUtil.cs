using System.Globalization;
using System.Linq;
using Discord.Commands;

namespace RoosterBot {
	public static class CommandUtil {
		/// <summary>
		/// Returns a localized signature of a CommandInfo.
		/// </summary>
		public static string GetSignature(this CommandInfo command, ResourceService resources, CultureInfo culture) {
			ComponentBase component = Program.Instance.Components.GetComponentForModule(command.Module);

			string ret = "";
			if (!string.IsNullOrWhiteSpace(command.Module.Group)) {
				ret += resources.ResolveString(culture, component, command.Module.Group + " ");
			}

			if (!string.IsNullOrWhiteSpace(command.Name)) {
				ret += resources.ResolveString(culture, component, command.Name + " ");
			}

			foreach (ParameterInfo param in command.Parameters) {
				ret += "<" + resources.ResolveString(culture, component, param.Name);
				RoosterTypeReader? reader = command.Module.Service.TypeReaders[param.Type].OfType<RoosterTypeReader>().FirstOrDefault();
				if (reader != null) {
					ComponentBase readerComponent = Program.Instance.Components.GetComponentFromAssembly(reader.GetType().Assembly);
					ret += ": " + resources.ResolveString(culture, readerComponent, reader.TypeDisplayName);
				} else {
					ret += ": " + param.Type.Name;
				}

				if (param.IsOptional) {
					ret += " = " + param.DefaultValue?.ToString() ?? "null";
				}
				ret += "> ";
			}

			if (ret.Length >= 1 && ret[^1] == ' ') {
				return ret[0..^2];
			} else {
				return ret;
			}
		}

		/// <summary>
		/// Returns a non-localized signature of a CommandInfo.
		/// </summary>
		public static string GetSignature(this CommandInfo command) {
			string ret = "";
			if (!string.IsNullOrWhiteSpace(command.Module.Group)) {
				ret += command.Module.Group + " ";
			}

			if (!string.IsNullOrWhiteSpace(command.Name)) {
				ret += command.Name + " ";
			}

			foreach (ParameterInfo param in command.Parameters) {
				ret += "<" + param.Name + ": " + param.Type.Name;
				if (param.IsOptional) {
					ret += " = " + param.DefaultValue?.ToString() ?? "null";
				}
				ret += "> ";
			}

			if (ret.Length >= 1 && ret[^1] == ' ') {
				return ret[0..^2];
			} else {
				return ret;
			}
		}
	}
}
