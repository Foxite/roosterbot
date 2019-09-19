using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[LogTag("MetaModule"), Name("#MetaCommandsModule_Name")]
	public class HelpModule : EditableCmdModuleBase {
		public HelpService Help { get; set; }

		[Command("help"), Summary("#MetaCommandsModule_HelpCommand_Summary")]
		public async Task HelpCommand() {
			string response = GetString("MetaCommandsModule_HelpCommand_HelpPretext", Config.CommandPrefix);

			bool notFirst = false;
			foreach (string helpSection in Help.GetSectionNames()) {
				if (notFirst) {
					response += ", ";
				}
				response += helpSection;
				notFirst = true;
			}

			response += GetString("MetaCommandsModule_HelpCommand_PostText");

			await ReplyAsync(response);
		}

		[Command("help"), Summary("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public async Task HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				(ComponentBase, string) helpSection = Help.GetHelpSection(section);
				response += ResourcesService.ResolveString(Culture, helpSection.Item1, helpSection.Item2);
			} else {
				response += GetString("MetaCommandsModule_HelpCommand_ChapterDoesNotExist");
			}
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("#MetaCommandsModule_CommandListCommand_Summary")]
		public async Task CommandListCommand() {
			// List modules with visible commands
			IEnumerable<string> visibleModules = 
				from module in CmdService.Modules
				where !module.Attributes.Any(attr => attr is HiddenFromListAttribute)
				select ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(module), module.Name).ToLower();

			string response = GetString("MetaCommandsModule_CommandListCommand_Pretext");
			response += visibleModules.Aggregate((workingString, next) => workingString + ", " + next);
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public async Task CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = CmdService.Modules
				.Where(aModule => ResourcesService.ResolveString(Culture, Program.Instance.Components.GetComponentForModule(aModule), aModule.Name).ToLower() == moduleName)
				.SingleOrDefault();

			if (module == null || module.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
				await ReplyAsync(GetString("MetaCommandsModule_CommandListCommand_CategoryDoesNotExist"));
			} else if (module.Commands.Count() == 0) {
				await ReplyAsync(GetString("MetaCommandsModule_CommandListCommand_CategoryEmpty"));
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
				await ReplyAsync(response);
			}
		}
	}
}
