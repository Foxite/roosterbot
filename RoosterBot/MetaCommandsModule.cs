using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public class MetaCommandsModule : ModuleBase {
		protected ConfigService Config { get; }
		protected ScheduleService Schedules { get; }
		protected CommandService CmdService { get; }
		protected DiscordSocketClient DiscordClient { get; }

		public MetaCommandsModule(ConfigService config, ScheduleService schedules, CommandService cmdService, DiscordSocketClient client) {
			CmdService = cmdService;
			Schedules = schedules;
			Config = config;
			DiscordClient = client;
		}

		[Command("help", RunMode = RunMode.Async)]
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
			Logger.Log(LogSeverity.Info, "MetaModule", "Shutting down");
			Program.Shutdown();
			return Task.CompletedTask;
		}

		[Command("reload", RunMode = RunMode.Async), RequireBotManager()]
		public async Task ReloadCSVCommand() {
			Logger.Log(LogSeverity.Info, "MetaModule", "Reloading config");
			Task<IUserMessage> progressMessage = ReplyAsync("Config herladen...");
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
				await DiscordClient.SetGameAsync(Config.GameString);
				Task.WaitAll(readCSVs);
				await (await progressMessage).ModifyAsync((msgProps) => { msgProps.Content = "OK."; });
			} catch (Exception ex) {
				try {
					Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reloading config.", ex);
					if (Config.ErrorReactions) {
						try {
							await Context.Message.AddReactionAsync(new Emoji("🚫"));
						} catch (HttpException) { } // Permission denied
					}
					await (await progressMessage).ModifyAsync((msgProps) => { msgProps.Content = "Critical error. Restart bot through AWS."; });
				} finally {
					await ShutdownCommand();
				}
			}
		}
	}

	public class RequireBotManagerAttribute : PreconditionAttribute {
		public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwnerId) {
				return PreconditionResult.FromSuccess();
			} else {
				if (services.GetService<ConfigService>().ErrorReactions) {
					try {
						await context.Message.AddReactionAsync(new Emoji("⛔"));
					} catch (HttpException) { } // Permission denied
				}
				return PreconditionResult.FromError("Je bent niet gemachtigd om dat te doen.");
			}
		}
	}
}
