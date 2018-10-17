using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using ScheduleComponent.Services;
using RoosterBot;
using RoosterBot.Modules;
using RoosterBot.Modules.Preconditions;

namespace ScheduleComponent.Modules {
	[RequireBotOperational]
	public class GenericCommandsModule : EditableCmdModuleBase {
		public CommandMatchingService MatchingService { get; set; }

		public GenericCommandsModule() : base() { }

		[Priority(10), Command("nu", RunMode = RunMode.Async)]
		public async Task GenericCurrentCommand([Remainder] string wat) {
			await MatchCommand(wat, "nu");
		}

		[Priority(10), Command("hierna", RunMode = RunMode.Async)]
		public async Task GenericNextCommand([Remainder] string wat) {
			await MatchCommand(wat, "hierna");
		}

		[Priority(10), Command("dag", RunMode = RunMode.Async)]
		public async Task GenericWeekdayCommand([Remainder] string wat) {
			await MatchCommand(wat, "dag");
		}

		[Priority(10), Command("morgen", RunMode = RunMode.Async)]
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
				return "klas " + command;
			case CommandType.Teacher:
				return "leraar " + command;
			case CommandType.Room:
				return "lokaal " + command;
			default:
				return null;
			}
		}
	}
}
