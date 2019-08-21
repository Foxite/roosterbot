using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Preconditions;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	[LogTag("MetaModule"), Name("Meta")]
	public class MetaCommandsModule : EditableCmdModuleBase {
		public HelpService Help { get; set; }

		[Command("help"), Summary("Uitleg over de bot.")]
		public async Task HelpCommand() {
			string response = string.Format(Resources.MetaCommandsModule_HelpCommand_HelpPretext, Config.CommandPrefix);

			bool notFirst = false;
			foreach (string helpSection in Help.GetSectionNames()) {
				if (notFirst) {
					response += ", ";
				}
				response += helpSection;
				notFirst = true;
			}

			response += Resources.MetaCommandsModule_HelpCommand_PostText;

			await ReplyAsync(response);
		}

		[Command("help"), Summary("Uitleg over een onderdeel van de bot.")]
		public async Task HelpCommand([Remainder, Name("hoofdstuk")] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				response += Help.GetHelpSection(section);
			} else {
				response += Resources.MetaCommandsModule_HelpCommand_ChapterDoesNotExist;
			}
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("Alle categorieën, of zoek op een categorie.")]
		public async Task CommandListCommand([Remainder, Name("categorie")] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = CmdService.Modules.Where(aModule => aModule.Name.ToLower() == moduleName).SingleOrDefault();

			if (module == null || module.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
				await ReplyAsync(Resources.MetaCommandsModule_CommandListCommand_CategoryDoesNotExist);
			} else if (module.Commands.Count() == 0) {
				await ReplyAsync(Resources.MetaCommandsModule_CommandListCommand_CategoryEmpty);
			} else {
				string response = "";

				int addedCommands = 0;
				// Commands
				foreach (CommandInfo command in module.Commands) {
					if (command.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
						continue;
					}

					response += $"`{Config.CommandPrefix}{command.Name}";

					// Parameters
					foreach (ParameterInfo param in command.Parameters) {
						if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						response += $" <{param.Name.Replace('_', ' ')}{(param.IsOptional ? "(?)" : "")}{(string.IsNullOrWhiteSpace(param.Summary) ? "" : $": {param.Summary}")}>";
					}
					response += $"`: {command.Summary}";

					// Preconditions
					if (command.Preconditions.Count() != 0) {
						string preconditionText = " (";
						int preconditionsAdded = 0;

						foreach (PreconditionAttribute pc in command.Preconditions) {
							if (pc is RoosterPreconditionAttribute rpc) {
								if (preconditionsAdded != 0) {
									preconditionText += ", ";
								}
								preconditionText += rpc.Summary;
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
					response += module.Remarks + "\n";
				}

				response += Resources.MetaCommandsModule_CommandListCommand_OptionalHint;
				await ReplyAsync(response);
			}
		}

		[Command("commands")]
		public async Task CommandListCommand() {
			// List modules with visible commands
			IEnumerable<string> visibleModules = 
				from module in CmdService.Modules
				where !module.Attributes.Any(attr => attr is HiddenFromListAttribute)
				select module.Name.ToLower();

			string response = Resources.MetaCommandsModule_CommandListCommand_Pretext;
			response += visibleModules.Aggregate((workingString, next) => workingString + ", " + next);
			await ReplyAsync(response);
		}

		[Command("shutdown"), RequireBotManager, HiddenFromList]
		public Task ShutdownCommand() {
			Log.Info($"Shutdown command used by {Context.User.Username}");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}
	}
}
