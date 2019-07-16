using System;
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
			string response = "Dit is RoosterBot " + Constants.VersionString + ".\n";

			response += $"Al mijn commands beginnen met `{Config.CommandPrefix}`. Hierdoor raken andere bots niet in de war.\n";
			response += "Je kan `!commands` gebruiken om te alle commands te zien, of `!commands <filter>` gebruiken om te zoeken.\n";
			response += "Gebruik `!help <hoofdstuk>` om specifieke uitleg te krijgen. Beschikbare hoofdstukken zijn:\n";

			bool notFirst = false;
			foreach (string helpSection in Help.GetSectionNames()) {
				if (notFirst) {
					response += ", ";
				}
				response += helpSection;
				notFirst = true;
			}

			await ReplyAsync(response);
		}

		[Command("help"), Summary("Uitleg over een onderdeel van de bot.")]
		public async Task HelpCommand([Remainder, Name("hoofdstuk")] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				response += Help.GetHelpSection(section);
			} else {
				response += "Sorry, dat hoofdstuk bestaat niet.";
			}
			await ReplyAsync(response);
		}

		[Command("commands"), Summary("Alle commands, of zoek op een command of categorie.")]
		public async Task CommandListCommand([Name("zoekterm")] string term = "") {
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
				Dictionary<string, string> moduleTexts = new Dictionary<string, string>();
				foreach (IGrouping<ModuleInfo, CommandInfo> group in groupedCommands) {
					if (group.Key.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
						continue;
					}

					string moduleResponse;
					if (moduleTexts.ContainsKey(group.Key.Name)) {
						moduleResponse = "";
					} else {
						moduleResponse = $"\n**{group.Key.Name}**: {group.Key.Summary}\n";
					}

					int addedCommands = 0;
					foreach (CommandInfo command in group) {
						if (command.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
							continue;
						}

						moduleResponse += $"`{Config.CommandPrefix}{command.Name}";
						foreach (ParameterInfo param in command.Parameters) {
							if (param.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
								continue;
							}

							moduleResponse += $" <{param.Name.Replace('_', ' ')}{(param.IsOptional ? "(?)" : "")}{(string.IsNullOrWhiteSpace(param.Summary) ? "" : $": {param.Summary}")}>";
						}
						moduleResponse += $"`: {command.Summary}";

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
						moduleResponse += "\n";
						addedCommands++;
					}

					if (!string.IsNullOrWhiteSpace(group.Key.Remarks)) {
						moduleResponse += group.Key.Remarks + "\n";
					}

					if (addedCommands != 0) {
						if (moduleTexts.ContainsKey(group.Key.Name)) {
							moduleTexts[group.Key.Name] += moduleResponse;
						} else {
							moduleTexts[group.Key.Name] = moduleResponse;
						}
					}
				}

				foreach (KeyValuePair<string, string> kvp in moduleTexts) {
					response += kvp.Value;
				}

				response += "Parameters met een `(?)` zijn optioneel.";
				await ReplyAsync(response);
			}
		}

		[Command("info"), Summary("Technische informatie over de bot")]
		public Task InfoCommand() {
			ReplyDeferred($"RoosterBot versie: {Constants.VersionString}");
			ReplyDeferred("Componenten:");

			foreach (KeyValuePair<Type, ComponentBase> kvp in Program.Instance.m_Components) {
				string componentName = kvp.Key.Name;
				if (componentName.EndsWith("Component")) {
					componentName = componentName.Substring(0, kvp.Key.Name.Length - "Component".Length);
				}
				ReplyDeferred(componentName + ": " + kvp.Value.VersionString);
			}

			return Task.CompletedTask;
		}

		[Command("shutdown"), RequireBotManager, HiddenFromList]
		public Task ShutdownCommand() {
			Log.Info("Shutting down");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}
	}
}
