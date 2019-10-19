using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class HelpModule : RoosterModuleBase {
		public HelpService Help { get; set; }

		[Command("help"), Summary("#MetaCommandsModule_HelpCommand_Summary")]
		public Task HelpCommand() {
			string response = GetString("MetaCommandsModule_HelpCommand_HelpPretext", GuildConfig.CommandPrefix) + "\n\n";
			response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n";
			response += string.Join(", ", Help.GetSectionNames()) + "\n\n";

			response += GetString("MetaCommandsModule_HelpCommand_PostText", GuildConfig.CommandPrefix);

			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("help"), Summary("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public Task HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				(ComponentBase, string) helpSection = Help.GetHelpSection(section);
				response += string.Format(ResourcesService.ResolveString(Culture, helpSection.Item1, helpSection.Item2), GuildConfig.CommandPrefix);
			} else {
				response += GetString("MetaCommandsModule_HelpCommand_ChapterDoesNotExist") + "\n\n";
				response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext") + "\n";
				response += string.Join(", ", Help.GetSectionNames());
			}
			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("commands"), Summary("#MetaCommandsModule_CommandListCommand_Summary")]
		public Task CommandListCommand() {
			ReplyDeferred(GetCategoryList());

			return Task.CompletedTask;
		}

		[Command("commands"), Summary("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public Task CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = (
				from aModule in CmdService.Modules
				where ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(aModule), aModule.Name).ToLower() == moduleName
				let culturePrecon = aModule.Preconditions.OfType<RequireCultureAttribute>().SingleOrDefault()
				where culturePrecon == null || !culturePrecon.Hide || culturePrecon.Culture.Equals(Culture)
				select aModule
				).SingleOrDefault();

			if (module == null || module.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
				string response = GetString("MetaCommandsModule_CommandListCommand_CategoryDoesNotExist");
				response += GetCategoryList();
				base.ReplyDeferred(response);
			} else if (module.Commands.Count() == 0) {
				ReplyDeferred(GetString("MetaCommandsModule_CommandListCommand_CategoryEmpty"));
			} else {
				string response = "";

				int addedCommands = 0;
				ComponentBase component = Program.Instance.Components.GetComponentForModule(module);

				// Commands
				foreach (CommandInfo command in module.Commands) {
					if (command.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
						continue;
					}

					response += $"`{GuildConfig.CommandPrefix}";
					bool groupAdded = false;
					if (!string.IsNullOrWhiteSpace(command.Module.Group)) {
						response += ResourcesService.ResolveString(Culture, component, command.Module.Group);
						groupAdded = true;

					}
					string name = ResourcesService.ResolveString(Culture, component, command.Name);
					if (!string.IsNullOrWhiteSpace(name)) {
						if (groupAdded) {
							response += " ";
						}
						response += name;
					}

					// Parameters
					foreach (ParameterInfo param in command.Parameters) {
						if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						string paramName = ResourcesService.ResolveString(Culture, component, param.Name);
						string paramSummary = string.IsNullOrWhiteSpace(param.Summary) ? "" : $": {string.Format(ResourcesService.ResolveString(Culture, component, param.Summary), GuildConfig.CommandPrefix)}";
						
						response += $" <{paramName.Replace('_', ' ')}{(param.IsOptional ? "(?)" : "")}{paramSummary}>";
					}

					if (string.IsNullOrWhiteSpace(command.Summary)) {
						response += "`";
					} else {
						string commandSummary = string.Format(ResourcesService.ResolveString(Culture, component, command.Summary), GuildConfig.CommandPrefix);
						response += $"`: {commandSummary}";
					}

					// Preconditions
					if (command.Preconditions.Count() != 0) {
						string preconditionText = " (";
						int preconditionsAdded = 0;

						foreach (PreconditionAttribute pc in command.Preconditions) {
							if (pc is RoosterPreconditionAttribute rpc) {
								if (preconditionsAdded != 0) {
									preconditionText += ", ";
								}
								preconditionText += string.Format(ResourcesService.ResolveString(Culture, component, rpc.Summary), GuildConfig.CommandPrefix);
								preconditionsAdded++;
							}
						}

						if (preconditionsAdded != 0) {
							response += preconditionText + ")";
						}
					}
					response += "\n";
					addedCommands++;
				}

				if (!string.IsNullOrWhiteSpace(module.Remarks)) {
					response += string.Format(ResourcesService.ResolveString(Culture, component, module.Remarks), GuildConfig.CommandPrefix) + "\n";
				}

				response += GetString("MetaCommandsModule_CommandListCommand_OptionalHint");
				ReplyDeferred(response);
			}

			return Task.CompletedTask;
		}

		private string GetCategoryList() {
			// List modules with visible commands
			IEnumerable<string> visibleModules =
				from module in CmdService.Modules
				where !module.Attributes.Any(attr => attr is HiddenFromListAttribute)
				let culture = module.Preconditions.OfType<RequireCultureAttribute>().FirstOrDefault()
				where culture == null || culture.Culture.Equals(Culture)
				select ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(module), module.Name).ToLower();

			string ret = GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext") + "\n";
			ret += string.Join(", ", visibleModules);
			return ret;
		}
	}
}
