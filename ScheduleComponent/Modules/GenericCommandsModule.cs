using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using ScheduleComponent.Services;
using RoosterBot;
using RoosterBot.Modules;
using RoosterBot.Attributes;

namespace ScheduleComponent.Modules {
	[LogTag("GenericCommandsModule"), Name("Rooster"), Summary("Begrijpt automatisch of je een klas, leraar, of lokaal bedoelt."),
		Remarks("Met `!ik` kun je instellen in welke klas jij zit, zodat je hier niets hoeft in te vullen.")]
	public class GenericCommandsModule : EditableCmdModuleBase {
		public CommandMatchingService MatchingService { get; set; }
		
		[Priority(10), Command("nu", RunMode = RunMode.Async), Alias("rooster"), Summary("Kijk wat er nu op het rooster staat.")]
		public async Task GenericCurrentCommand([Remainder] string wat = "ik") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await MinorError("Ik moet een klas, lokaal of leraar hebben.");
				return;
			}

			await MatchCommand(wat, "nu");
		}

		[Priority(10), Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("Kijk wat er hierna op het rooster staat")]
		public async Task GenericNextCommand([Remainder] string wat = "ik") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await MinorError("Ik moet een klas, lokaal of leraar hebben.");
				return;
			}
			
			await MatchCommand(wat, "hierna");
		}

		[Priority(10), Command("dag", RunMode = RunMode.Async), Summary("Het rooster voor een bepaalde dag. Als je de huidige dag gebruikt, pak ik volgende week. `!vandaag` doet dit niet.")]
		public async Task GenericWeekdayCommand([Remainder] string wat_en_weekdag) {
			if (string.IsNullOrWhiteSpace(wat_en_weekdag)) {
				await MinorError("Ik moet een klas, lokaal of leraar hebben, en een weekdag.");
				return;
			}

			await MatchCommand(wat_en_weekdag, "dag");
		}

		[Priority(10), Command("morgen", RunMode = RunMode.Async), Summary("Wat er morgen op het rooster staat.")]
		public async Task GenericTomorrowCommand([Remainder] string wat = "ik") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await MinorError("Ik moet een klas, lokaal of leraar hebben.");
				return;
			}

			await MatchCommand(wat, "morgen");
		}

		[Priority(10), Command("vandaag", RunMode = RunMode.Async), Summary("Wat er vandaag op het rooster staat.")]
		public async Task GenericTodayCommand([Remainder] string wat = "ik") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await MinorError("Ik moet een klas, lokaal of leraar hebben.");
				return;
			}

			await MatchCommand(wat, "vandaag");
		}

		private async Task MatchCommand(string parameters, string command) {
			if (command == "dag") {
				// `dag` command is made so that you can specify the day as either the first or last parameter. The identifier (student set, teacher, or room) may be any length.
				// So we have to check which one is the day, then we can assume the rest is the identifier.
				// Because it's easier (or seemed at the time, because I'm commenting this way after writing it), we do the opposite:
				// We take all the parameters *except* the first or last, and check if it mmatches a student set, teacher, or room (in another function).
				List<string> argumentsAsList = parameters.Split(' ').ToList();

				if (argumentsAsList.Count == 1) {
					parameters += " ik";
					argumentsAsList = parameters.Split(' ').ToList();
				}

				List<string> argumentsWithoutFirst = argumentsAsList.Select(item => (string) item.Clone()).ToList(); // clone list
				List<string> argumentsWithoutLast = argumentsAsList.Select(item => (string) item.Clone()).ToList(); // from https://stackoverflow.com/a/222640/3141917
				argumentsWithoutFirst.RemoveAt(0);
				argumentsWithoutLast.RemoveAt(argumentsAsList.Count - 1);
				string paramWithoutFirst = string.Join(" ", argumentsWithoutFirst);
				string paramWithoutLast = string.Join(" ", argumentsWithoutLast);

				// Here we call a function that does the matching for us, it will return the correct command call based on the parameters. If it can't figure it out, it returns null.
				// We first try it without the first parameter (to see if that one is the weekday).
				string result = MatchCommandNoWeekday(paramWithoutFirst, command);
				if (result != null) {
					await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + parameters, Context.Message);
				} else {
					// If it's null, it may be that the last parameter is the weekday.
					result = MatchCommandNoWeekday(paramWithoutLast, command);
					if (result != null) {
						await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, result + " " + parameters, Context.Message);
					} else {
						// If it still returns null, the identifier was not recognized by the function.
						await MinorError("Ik weet niet wat je bedoelt met \"" + parameters + "\".");
					}
				}
			} else {
				// For all other commands we can just parse the command parameters directly.
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
