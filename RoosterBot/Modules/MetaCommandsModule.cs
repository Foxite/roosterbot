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

		[Command("help", RunMode = RunMode.Async), Summary("Uitleg over een onderdeel van de bot.")]
		public async Task HelpCommand([Remainder, Summary("hoofdstuk"), Name("hoofdstuk")] string section = "") {
			string response = "";

			if (string.IsNullOrWhiteSpace(section)) {
				response += "Al mijn commands beginnen met een `!`. Hierdoor raken andere bots niet in de war.\n\n";
				response += "Gebruik `!help <hoofdstuk>`. Beschikbare hoofdstukken zijn:\n";

				bool notFirst = false;
				foreach (string helpSection in Help.GetSectionNames()) {
					if (notFirst) {
						response += ", ";
					}
					response += helpSection;
					notFirst = true;
				}

				response += "\n\nDit is versie " + Constants.VersionString + ".";
			} else {
				if (Help.HelpSectionExists(section)) {
					response += Help.GetHelpSection(section);
				} else {
					response += "Sorry, dat hoofdstuk bestaat niet.";
				}
			}
			
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("Alle commands, of zoek op een command of categorie.")]
		public async Task CommandListCommand([Summary("Een command of categorie"), Name("zoekterm")] string term = "") {
			IEnumerable<CommandInfo> commands = CmdService.Commands;
			
			if (!string.IsNullOrWhiteSpace(term)) {
				// Filter list
				term = term.ToLower();
				commands = commands.Where(command => command.Name.ToLower().Contains(term) || command.Module.Name.ToLower().Contains(term));
			}

			if (commands.Count() == 0) {
				await ReplyAsync("Geen commands gevonden.");
			} else {
				IEnumerable<IGrouping<ModuleInfo, CommandInfo>> groupedCommands = commands.GroupBy(command => command.Module);

				string response = "";
				foreach (IGrouping<ModuleInfo, CommandInfo> group in groupedCommands) {
					if (group.Key.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
						continue;
					}

					response += $"**{group.Key.Name}**: {group.Key.Summary}\n";
					foreach (CommandInfo command in group) {
						if (command.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						response += $"`{Config.CommandPrefix}{command.Name}";
						foreach (ParameterInfo param in command.Parameters) {
							if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
								continue;
							}

							response += $" {param.Name}{(param.IsOptional ? "(?)" : "")}";
						}
						response += $"`: {command.Summary}";

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
					}
					response += "\n";
				}
				response += "Parameters met een `(?)` zijn optioneel.";
				await ReplyAsync(response);
			}
		}

		[Command("shutdown"), RequireBotManager, HiddenFromList]
		public Task ShutdownCommand() {
			Log.Info("Shutting down");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}
	}
}
