using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Modules.Preconditions;

namespace RoosterBot.Modules {
	[RoosterBot.Attributes.LogTag("MCM")]
	public class MetaCommandsModule : EditableCmdModuleBase {
		[Command("help", RunMode = RunMode.Async)]
		public async Task HelpCommand() {
			if (!await CheckCooldown(1f))
				return;

			// Print list of commands
			// TODO allow components to define their own help text
			string response = "Al mijn commands beginnen met een `!`. Hierdoor raken andere bots niet in de war.\n\n";
			response += "Je kan opvragen welke les een klas of een leraar nu heeft, of in een lokaal bezig is.\n";
			response += "Ik begrijp dan automatisch of je het over een klas, leraar of lokaal hebt.\n";
			response += "Ik ken de afkortingen, voornamen, en alternative spellingen van alle leraren.\n";
			response += "`!nu 1gd2`, `!nu laurence candel`, `!nu laurens`, `!nu lca`, `!nu a223`\n\n";
			response += "Je kan ook opvragen wat er hierna, op een bepaalde weekdag, of morgen als eerste is.\n";
			response += "`!hierna 2gd1`, `!dag lance woensdag` (de volgorde maakt niet uit: `!dag wo lkr` doet hetzelfde), `!morgen b114`\n\n";
			response += "Je kan ook zien wat de klas/leraar/lokaal heeft na wat ik je net heb verteld. Dus als je pauze hebt, kun je zien wat je na de pauze hebt.\n";
			response += "`!hierna 3ga1` en dan `!daarna`. Je kan `!daarna` zo vaak gebruiken als je wilt.\n\n";
			response += "Als ik niet begrijp of je het over een klas, leraar, of lokaal hebt, kun je dit in de command zetten:\n";
			response += "`!klas nu 2ga1`, `leraar dag martijn dinsdag`, `!lokaal morgen a128`\n\n";
			response += "Je kan een lijst van alle docenten opvragen, met hun afkortingen en discord namen: `!docenten` of `!leraren`";
			await ReplyAsync(response);
		}

		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Logger.Log(LogSeverity.Info, "MetaModule", "Shutting down");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}
	}
}
