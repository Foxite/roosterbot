using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RoosterBot.Modules.Preconditions;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public class MetaCommandsModule : EditableCmdModuleBase {
		public ScheduleService Schedules { get; set; }
		public TeacherNameService Teachers { get; set; }

		public MetaCommandsModule() : base() {
			LogTag = "MCM";
		}

		[Command("help", RunMode = RunMode.Async), RequireBotOperational]
		public async Task HelpCommand() {
			if (!await CheckCooldown())
				return;

			// Print list of commands
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
			response += "`!klas nu 2ga1`, `leraar dag martijn dinsdag`, `!lokaal morgen a128`";
			await ReplyAsync(response);
		}

		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Logger.Log(LogSeverity.Info, "MetaModule", "Shutting down");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}

		[Command("reload", RunMode = RunMode.Async), RequireBotOperational, RequireBotManager]
		public async Task ReloadCSVCommand() {
			Logger.Log(LogSeverity.Info, "MetaModule", "Reloading config");
			Task<IUserMessage> progressMessage = ReplyAsync("Config herladen...");
			try {
				Program.Instance.State = ProgramState.BeforeStart;
				var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoosterBot");

				// Clear everything
				Schedules.Reset();
				Teachers.Reset();

				// Reload
				Config.ReloadConfig(Path.Combine(configPath, "Config.Json"), out Dictionary<string, string> schedules);

				List<Task> concurrentLoading = new List<Task>();
				int i = 0;
				foreach (KeyValuePair<string, string> schedule in schedules) {
					concurrentLoading.Add(Schedules.ReadScheduleCSV(schedule.Key, Path.Combine(configPath, schedule.Value)));
					i++;
				}
				concurrentLoading.Add(Teachers.ReadAbbrCSV(Path.Combine(configPath, "leraren-afkortingen.csv")));
				concurrentLoading.Add((Context.Client as DiscordSocketClient)?.SetGameAsync(Config.GameString));
				
				Task.WaitAll(concurrentLoading.ToArray());

				Program.Instance.State = ProgramState.BotRunning;

				await (await progressMessage).ModifyAsync((msgProps) => { msgProps.Content = "OK."; });
			} catch (Exception ex) {
				try {
					Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reloading config.", ex);
					if (Config.ErrorReactions) {
						await AddReaction("🚫");
					}
					await (await progressMessage).ModifyAsync((msgProps) => { msgProps.Content = "Critical error. Bot shutting down."; });
				} finally {
					await ShutdownCommand();
				}
			}
		}
	}
}
