using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// This class has 2 purposes:
	// - Make it possible for a missing parameter to be resolved to "ik"
	// - Provide a more useful error message in case a parameter is not understood
	// 
	// The first one can conceivably be accomplished by providing a default value, however, this is hard because default parameter types have to be compile-time constants.
	// The second one is still valid, but in theory, there needs to be a way to do this without having a separate module.
	// 
	// Neither of these necessities are the result of writing good code, so there is a long-standing todo item:
	// TODO: make this class obsolete.
	[LogTag("DefaultCommandsModule"), HiddenFromList]
	public class DefaultScheduleModule : EditableCmdModuleBase {
		[Priority(-10), Command("nu", RunMode = RunMode.Async)]
		public async Task DefaultCurrentCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "nu ik", "default nu");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("hierna", RunMode = RunMode.Async)]
		public async Task DefaultNextCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "hierna ik", "default hierna");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Async)]
		public async Task DefaultWeekdayCommand(DayOfWeek dag, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, $"dag {dag.GetName(Culture)} ik", "default dag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("morgen", RunMode = RunMode.Async)]
		public async Task DefaultTomorrowCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "morgen ik", "default morgen");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("vandaag", RunMode = RunMode.Async)]
		public async Task DefaultTodayCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "vandaag ik", "default vandaag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("deze week", RunMode = RunMode.Async)]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "deze week ik", "default deze week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("volgende week", RunMode = RunMode.Async)]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, "volgende week ik", "default volgende week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("over", RunMode = RunMode.Async)]
		public async Task ShowNWeeksWorkingDaysCommand(int aantal, string unit, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Util.ExecuteSpecificCommand(CmdService, Context, $"over {aantal} {unit} ik", "default over");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		private async Task ReplyErrorMessage(string param) {
			if (MentionUtils.TryParseUser(param, out _)) {
				await ReplyAsync(GetString("DefaultScheduleModule_ReplyErrorMessage_MentionUserUnknown"));
			} else if (param == "ik") {
				await ReplyAsync(GetString("DefaultScheduleModule_ReplyErrorMessage_UserUnknown"));
			} else {
				await ReplyAsync(GetString("DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier"));
			}
		}
	}
}
