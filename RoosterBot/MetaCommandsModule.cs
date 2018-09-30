using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public class MetaCommandsModule : ModuleBase {
		protected ConfigService Config { get; }
		protected CommandService CmdService { get;  }

		public MetaCommandsModule(ConfigService config, CommandService cmdService) {
			CmdService = cmdService;
			Config = config;
		}

		[Command("help"), Summary("Lijst van alle commands")]
		public async Task HelpCommand() {
			// Print list of commands
			string response = "Commands die je bij mij kan gebruiken:\n";
			foreach (CommandInfo cmd in CmdService.Commands) {
				if (cmd.Name == "halt" || cmd.Name == "shutdown") {
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

		[Command("halt", RunMode = RunMode.Async), RequireBotManager()]
		public Task HaltCommand() {
			Environment.Exit(1);
			return Task.CompletedTask; // ...
		}

		[Command("shutdown", RunMode = RunMode.Async), RequireBotManager()]
		public Task ShutdownCommand() {
			Program.Shutdown();
			return Task.CompletedTask;
		}
	}

	public class RequireBotManagerAttribute : PreconditionAttribute {
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwnerId) {
				var task = new Task<PreconditionResult>(() => PreconditionResult.FromSuccess());
				task.Start();
				return task;
			} else {
				var task = new Task<PreconditionResult>(() => PreconditionResult.FromError("Je bent niet gemachtigd om dat te doen."));
				task.Start();
				return task;
			}
		}
	}
}
