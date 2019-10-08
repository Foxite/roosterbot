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
			string response = GetString("MetaCommandsModule_HelpCommand_HelpPretext", Config.CommandPrefix) + "\n\n";
			response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext") + "\n";
			response += string.Join(", ", Help.GetSectionNames()) + "\n\n";

			response += GetString("MetaCommandsModule_HelpCommand_PostText");

			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("help"), Summary("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public Task HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				(ComponentBase, string) helpSection = Help.GetHelpSection(section);
				response += ResourcesService.ResolveString(Culture, helpSection.Item1, helpSection.Item2);
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
			// List modules with visible commands
			string response = GetString("MetaCommandsModule_CommandListCommand_Pretext") + "\n\n";
			response += GetCategoryList();

			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("commands"), Summary("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public Task CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = CmdService.Modules
				.Where(aModule => ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(aModule), aModule.Name).ToLower() == moduleName)
				.SingleOrDefault();

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

					response += $"`{Config.CommandPrefix}";
					response += ResourcesService.ResolveString(Culture, component, command.Name);

					// Parameters
					foreach (ParameterInfo param in command.Parameters) {
						if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						string paramName = ResourcesService.ResolveString(Culture, component, param.Name);
						string paramSummary = string.IsNullOrWhiteSpace(param.Summary) ? "" : $": {ResourcesService.ResolveString(Culture, component, param.Summary)}";
						
						response += $" <{paramName.Replace('_', ' ')}{(param.IsOptional ? "(?)" : "")}{paramSummary}>";
					}

					if (string.IsNullOrWhiteSpace(command.Summary)) {
						response += "`";
					} else {
						string commandSummary = ResourcesService.ResolveString(Culture, component, command.Summary);
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
								preconditionText += ResourcesService.ResolveString(Culture, component, rpc.Summary);
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
					response += ResourcesService.ResolveString(Culture, component, module.Remarks) + "\n";
				}

				response += GetString("MetaCommandsModule_CommandListCommand_OptionalHint");
				ReplyDeferred(response);
			}

			return Task.CompletedTask;
		}

		private string GetCategoryList() {
			IEnumerable<string> visibleModules =
				from module in CmdService.Modules
				where !module.Attributes.Any(attr => attr is HiddenFromListAttribute)
				select ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(module), module.Name).ToLower();

			string ret = GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext") + "\n";
			ret += string.Join(", ", visibleModules);
			return ret;
		}
	}
}
