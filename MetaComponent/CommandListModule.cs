using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Meta {
	public class CommandListModule : EditableCmdModuleBase {
		[Command("commands"), Summary("#" + nameof(Resources.MetaCommandsModule_CommandListCommand_Summary))]
		public async Task CommandListCommand() {
			// List modules with visible commands
			IEnumerable<string> visibleModules = 
				from module in CmdService.Modules
				where !module.Attributes.Any(attr => attr is HiddenFromListAttribute)
				select Util.ResolveString(Program.Instance.Components.GetComponentForModule(module), module.Name).ToLower();

			string response = Resources.MetaCommandsModule_CommandListCommand_Pretext;
			response += visibleModules.Aggregate((workingString, next) => workingString + ", " + next);
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("#" + nameof(Resources.MetaCommandsModule_CommandListCommand_Category_Summary))]
		public async Task CommandListCommand([Remainder, Name("#" + nameof(Resources.MetaCommandsModule_CommandListCommand_ModuleName))] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = CmdService.Modules
				.Where(aModule => Util.ResolveString(Program.Instance.Components.GetComponentForModule(aModule), aModule.Name).ToLower() == moduleName)
				.SingleOrDefault();

			if (module == null || module.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
				await ReplyAsync(Resources.MetaCommandsModule_CommandListCommand_CategoryDoesNotExist);
			} else if (module.Commands.Count() == 0) {
				await ReplyAsync(Resources.MetaCommandsModule_CommandListCommand_CategoryEmpty);
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
					response += Util.ResolveString(component, command.Name);

					// Parameters
					foreach (ParameterInfo param in command.Parameters) {
						if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						string paramName = Util.ResolveString(component, param.Name);
						string paramSummary = string.IsNullOrWhiteSpace(param.Summary) ? "" : $": {Util.ResolveString(component, param.Summary)}";
						
						response += $" <{paramName.Replace('_', ' ')}{(param.IsOptional ? "(?)" : "")}{paramSummary}>";
					}

					if (string.IsNullOrWhiteSpace(command.Summary)) {
						response += "`";
					} else {
						string commandSummary = Util.ResolveString(component, command.Summary);
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
								preconditionText += Util.ResolveString(component, rpc.Summary);
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
					response += Util.ResolveString(component, module.Remarks) + "\n";
				}

				response += Resources.MetaCommandsModule_CommandListCommand_OptionalHint;
				await ReplyAsync(response);
			}
		}
	}
}
