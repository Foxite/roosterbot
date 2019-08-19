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
			string response = $"Al mijn commands beginnen met `{Config.CommandPrefix}`. Hierdoor raken andere bots niet in de war.\n";
			response += "Gebruik `!help <hoofdstuk>` om specifieke uitleg te krijgen. Beschikbare hoofdstukken zijn:\n";

			bool notFirst = false;
			foreach (string helpSection in Help.GetSectionNames()) {
				if (notFirst) {
					response += ", ";
				}
				response += helpSection;
				notFirst = true;
			}


			response += "\nJe kan `!commands` gebruiken om een lijst van onderdelen te zien, en `!commands <onderdeel>` gebruiken om alle commands te zien.\n";

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

		[Command("commands"), Summary("Alle categorieën, of zoek op een categorie.")]
		public async Task CommandListCommand([Remainder, Name("categorie")] string moduleName) {
			moduleName = moduleName.ToLower();
			ModuleInfo module = CmdService.Modules.Where(aModule => aModule.Name.ToLower() == moduleName).SingleOrDefault();

			if (module == null || module.Attributes.Any(attr => attr is HiddenFromListAttribute)) {
				await ReplyAsync("Die categorie bestaat niet.");
			} else if (module.Commands.Count() == 0) {
				await ReplyAsync("Geen commands gevonden.");
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

				response += "Parameters met een `(?)` zijn optioneel.";
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

			string response = "Beschikbare onderdelen zijn:\n";
			response += visibleModules.Aggregate((workingString, next) => workingString + ", " + next);
			await ReplyAsync(response);
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
