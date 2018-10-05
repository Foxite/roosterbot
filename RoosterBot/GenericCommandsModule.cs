using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot {
	public class GenericCommandsModule : EditableCmdModuleBase {
		public CommandMatchingService MatchingService { get; set; }

		[Command("nu", RunMode = RunMode.Async)]
		public async Task GenericCurrentCommand([Remainder] string wat) {
			await MatchCommand(wat, "nu"); // TODO display how much time is left
		}

		[Command("hierna", RunMode = RunMode.Async)]
		public async Task GenericNextCommand([Remainder] string wat) {
			await MatchCommand(wat, "hierna");
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task GenericWeekdayCommand([Remainder] string wat) {
			await MatchCommand(wat, "dag");
		}

		[Command("morgen", RunMode = RunMode.Async)]
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
					await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + argumentsAsList[0], Context.Message);
				} else {
					result = MatchCommandNoWeekday(paramWithoutLast, command);
					if (result != null) {
						await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + argumentsAsList[argumentsAsList.Count - 1], Context.Message);
					} else {
						await FatalError("Ik weet niet wat je bedoelt met \"" + parameters + "\".");
					}
				}
			} else {
				string result = MatchCommandNoWeekday(parameters, command);
				if (result == null) {
					await FatalError("Ik weet niet wat je bedoelt met \"" + parameters + "\".");
				} else {
					await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result, Context.Message);
				}
			}
		}

		private string MatchCommandNoWeekday(string parameters, string command) {
			switch (MatchingService.MatchCommand(parameters)) {
			case CommandType.Student:
				return "leerling" + command + " " + parameters;
			case CommandType.Teacher:
				return "leraar" + command + " " + parameters;
			case CommandType.Room:
				return "lokaal" + command + " " + parameters;
			default:
				return null;
			}
		}

		private async Task FatalError(string message) {
			await ReplyAsync(message);

		}
	}
}
