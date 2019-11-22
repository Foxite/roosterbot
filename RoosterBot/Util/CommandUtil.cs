using System;
using System.Globalization;
using System.Linq;
using Discord.Commands;
using Qmmands;

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
		public static string GetSignature(this Command command, ResourceService resources, CultureInfo culture) {
			ComponentBase moduleComponent = Program.Instance.Components.GetComponentForModule(command.Module);

			string ret = "";
			if (!string.IsNullOrWhiteSpace(command.Module.Group)) {
				ret += resources.ResolveString(culture, moduleComponent, command.Module.Group) + " ";
			}

			if (!string.IsNullOrWhiteSpace(command.Name)) {
				ret += resources.ResolveString(culture, moduleComponent, command.Name) + " ";
			}
			foreach (ParameterInfo param in command.Parameters) {
				if (param.Name != "ignored") {
					string paramText = resources.ResolveString(culture, moduleComponent, param.Name);

					RoosterTypeReader? typeReader = param.Command.Module.Service.TypeReaders.SelectMany(g => g).OfType<RoosterTypeReader>().Where(rtr => rtr.Type == param.Type).FirstOrDefault();
					TypeDisplayAttribute? typeDisplayAttr = param.Attributes.OfType<TypeDisplayAttribute>().SingleOrDefault();
					ComponentBase? typeReaderComponent = typeReader == null ? null : Program.Instance.Components.GetComponentFromAssembly(typeReader.GetType().Assembly);
					if (typeDisplayAttr != null) {
						paramText += ": " + resources.ResolveString(culture, moduleComponent, typeDisplayAttr.TypeDisplayName);
					} else if (typeReader != null) {
						string typeDisplayName = resources.ResolveString(culture, typeReaderComponent, typeReader.TypeDisplayName);
						if (typeDisplayName != param.Name) {
							// Only include the type if the name isn't the same as the type display name
							paramText += ": " + typeDisplayName;
						}
					} else {
						paramText += ": " + GetNiceName(param.Type);
					}
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

		/// <summary>
		/// Returns a non-localized signature of a CommandInfo.
		/// </summary>
		public static string GetSignature(this Command command) {
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
