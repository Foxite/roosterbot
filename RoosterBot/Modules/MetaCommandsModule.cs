using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Modules.Preconditions;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	[Attributes.LogTag("MetaModule")]
	public class MetaCommandsModule : EditableCmdModuleBase {
		public HelpService Help { get; set; }

		[Command("help", RunMode = RunMode.Async)]
		public async Task HelpCommand([Remainder] string section = "") {
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

		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Log.Info("Shutting down");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}
	}
}
