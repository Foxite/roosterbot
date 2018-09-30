using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public class MetaCommandsModule : ModuleBase {
		protected ConfigService Config { get; }
		protected ScheduleService Schedules { get; }
		protected CommandService CmdService { get; }

		public MetaCommandsModule(ConfigService config, ScheduleService schedules, CommandService cmdService) {
			CmdService = cmdService;
			Schedules = schedules;
			Config = config;
		}

		[Command("help")]
		public async Task HelpCommand() {
			// Print list of commands
			string response = "Commands die je bij mij kan gebruiken:\n";
			foreach (CommandInfo cmd in CmdService.Commands) {
				if (cmd.Module.Name == this.GetType().Name) {
					continue;
				}
				response += "- `" + Config.CommandPrefix + cmd.Name;
				foreach (ParameterInfo parameter in cmd.Parameters) {
					response += $" <{parameter.Name}>";
				}
				response += $"`: {cmd.Summary}\n";
			}
			await ReplyAsync(response);
		}

		[Command("shutdown"), RequireBotManager()]
		public Task ShutdownCommand() {
			Program.Shutdown();
			return Task.CompletedTask;
		}

		[Command("reload", RunMode = RunMode.Async), RequireBotManager()]
		public async Task ReloadCSVCommand() {
			try {
				var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoosterBot");
				Config.ReloadConfig(Path.Combine(configPath, "Config.Json"),
					out Dictionary<string, string> schedules);
				Schedules.Reset();
				Task[] readCSVs = new Task[schedules.Count];
				int i = 0;
				foreach (KeyValuePair<string, string> schedule in schedules) {
					readCSVs[i] = Schedules.ReadScheduleCSV(schedule.Key, Path.Combine(configPath, schedule.Value));
					i++;
				}
				Task.WaitAll(readCSVs);
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reloading config.", ex);
				if (Config.ErrorReactions) {
					await Context.Message.AddReactionAsync(new Emoji("🚫"));
				}
				await ReplyAsync("Critical error. Restart bot through AWS.");
				await ShutdownCommand();
			}
		}
	}

	public class RequireBotManagerAttribute : PreconditionAttribute {
		public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwnerId) {
				return PreconditionResult.FromSuccess();
			} else {
				if (services.GetService<ConfigService>().ErrorReactions) {
					await context.Message.AddReactionAsync(new Emoji("⛔"));
				}
				return PreconditionResult.FromError("Je bent niet gemachtigd om dat te doen.");
			}
		}
	}
}
