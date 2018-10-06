using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;

namespace RoosterBot {
	public class GenericCommandsModule : EditableCmdModuleBase {
		public CommandMatchingService MatchingService { get; set; }
		public LastScheduleCommandService LastService { get; set; }
		public ConfigService Config { get; set; }

		[Priority(1), Command("nu", RunMode = RunMode.Async)]
		public async Task GenericCurrentCommand([Remainder] string wat) {
			await MatchCommand(wat, "nu"); // TODO display how much time is left
		}

		[Priority(1), Command("hierna", RunMode = RunMode.Async)]
		public async Task GenericNextCommand([Remainder] string wat) {
			await MatchCommand(wat, "hierna");
		}

		[Priority(1), Command("dag", RunMode = RunMode.Async)]
		public async Task GenericWeekdayCommand([Remainder] string wat) {
			await MatchCommand(wat, "dag");
		}

		[Priority(1), Command("morgen", RunMode = RunMode.Async)]
		public async Task GenericTomorrowCommand([Remainder] string wat) {
			await MatchCommand(wat, "morgen");
		}

		private async Task MatchCommand(string parameters, string command) {
			if (command == "dag") {
				List<string> argumentsAsList = parameters.Split(' ').ToList();
				List<string> argumentsWithoutFirst = argumentsAsList.Select(item => (string) item.Clone()).ToList(); // clone list
				List<string> argumentsWithoutLast  = argumentsAsList.Select(item => (string) item.Clone()).ToList(); // from https://stackoverflow.com/a/222640/3141917
				argumentsWithoutFirst.RemoveAt(0);
				argumentsWithoutLast .RemoveAt(argumentsAsList.Count - 1);
				string paramWithoutFirst = string.Join(" ", argumentsWithoutFirst);
				string paramWithoutLast  = string.Join(" ", argumentsWithoutLast);

				string result = MatchCommandNoWeekday(paramWithoutFirst, command);
				if (result != null) {
					await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + parameters, Context.Message);
				} else {
					result = MatchCommandNoWeekday(paramWithoutLast, command);
					if (result != null) {
						await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + parameters, Context.Message);
					} else {
						await MinorError("Ik weet niet wat je bedoelt met \"" + parameters + "\".");
					}
				}
			} else {
				string result = MatchCommandNoWeekday(parameters, command);
				if (result == null) {
					await MinorError("Ik weet niet wat je bedoelt met \"" + parameters + "\".");
				} else {
					await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + parameters, Context.Message);
				}
			}
		}

		private string MatchCommandNoWeekday(string parameters, string command) {
			switch (MatchingService.MatchCommand(parameters)) {
			case CommandType.Student:
				return "leerling" + command;
			case CommandType.Teacher:
				return "leraar" + command;
			case CommandType.Room:
				return "lokaal" + command;
			default:
				return null;
			}
		}

		private async Task MinorError(string message) {
			LastService.RemoveLastQuery(Context.User);
			if (Config.ErrorReactions) {
				await AddReaction("❌");
			}
			await ReplyAsync(message);
		}

		protected async Task AddReaction(string unicode) {
			try {
				await Context.Message.AddReactionAsync(new Emoji(unicode));
			} catch (HttpException) { } // Permission denied
		}
	}
}
