using System;
using System.Globalization;
using System.Linq;
using Discord.Commands;

namespace RoosterBot {
	public static class CommandUtil {
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
		public static string GetSignature(this CommandInfo command, ResourceService resources, CultureInfo culture) {
			ComponentBase component = Program.Instance.Components.GetComponentForModule(command.Module);

			string ret = "";
			if (!string.IsNullOrWhiteSpace(command.Module.Group)) {
				ret += resources.ResolveString(culture, component, command.Module.Group) + " ";
			}

			if (!string.IsNullOrWhiteSpace(command.Name)) {
				ret += resources.ResolveString(culture, component, command.Name) + " ";
			}

			foreach (ParameterInfo param in command.Parameters) {
				string paramLine = resources.ResolveString(culture, component, param.Name);

				RoosterTypeReader? typeReader = param.Command.Module.Service.TypeReaders.SelectMany(g => g).OfType<RoosterTypeReader>().Where(rtr => rtr.Type == param.Type).FirstOrDefault();
				if (typeReader == null) {
					paramLine += ": " + GetNiceName(param.Type);
				} else if (typeReader.TypeDisplayName == param.Name) {
					// Only include the type if the name isn't the same as the type display name
					paramLine += ": " + resources.ResolveString(culture, component, typeReader.TypeDisplayName);
				}
				if (param.IsMultiple) {
					paramLine += "...";
				}

				if (param.IsOptional) {
					paramLine = "[" + paramLine + "] ";
				} else {
					paramLine = "<" + paramLine + "> ";
				}
				ret += paramLine;
			}
			return ret;
			/*
			if (ret.Length >= 1 && ret[^1] == ' ') {
				return ret[0..^2];
			} else {
				return ret;
			}*/
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
				string paramLine = param.Name;

				RoosterTypeReader? typeReader = param.Command.Module.Service.TypeReaders.SelectMany(g => g).OfType<RoosterTypeReader>().Where(rtr => rtr.Type == param.Type).FirstOrDefault();
				if (typeReader == null) {
					paramLine += ": " + GetNiceName(param.Type);
				} else if (typeReader.TypeDisplayName == param.Name) {
					// Only include the type if the name isn't the same as the type display name
					paramLine += ": " + typeReader.TypeDisplayName;
				}
				if (param.IsMultiple) {
					paramLine += "...";
				}

				if (param.IsOptional) {
					paramLine = "[" + paramLine + "] ";
				} else {
					paramLine = "<" + paramLine + "> ";
				}
				ret += paramLine;
			}

			return ret;
		}
	}
}
