using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// Provides several "virtual" commands that don't do much, but serve as a single list item for 3 different "versions" of each command
	// They also replace missing IdentifierInfo parameters with "ik" and re-execute the command,
	//  and provide an error message in case they are invoked and no TypeReader could figure out which version was to be used.
	[LogTag("DefaultCommandsModule"), Name("Rooster"),
		Summary("Begrijpt automatisch of je een klas, leraar, of lokaal bedoelt."),
		Remarks("Met `!ik` kun je instellen in welke klas jij zit, zodat je hier niets hoeft in te vullen.")]
	public class DefaultScheduleModule : EditableCmdModuleBase {
		[Priority(-10), Command("nu", RunMode = RunMode.Sync), Alias("rooster"), Summary("#" + nameof(Resources.DefaultScheduleModule_DefaultCurrentCommand_Summary))]
		public async Task DefaultCurrentCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "nu ik", Context.Message, "default nu");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		private async Task ReplyErrorMessage(string param) {
			if (MentionUtils.TryParseUser(param, out _)) {
				await ReplyAsync(Resources.DefaultScheduleModule_ReplyErrorMessage_MentionUserUnknown);
			} else if (param == "ik") {
				await ReplyAsync(Resources.DefaultScheduleModule_ReplyErrorMessage_UserUnknown);
			} else {
				await ReplyAsync(Resources.DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier);
			}
		}

		[Priority(-10), Command("hierna", RunMode = RunMode.Sync), Alias("later", "straks", "zometeen"), Summary("#" + nameof(Resources.DefaultScheduleModule_DefaultNextCommand_Summary))]
		public async Task DefaultNextCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "hierna ik", Context.Message, "default hierna");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Sync), Summary("#" + nameof(Resources.DefaultScheduleModule_DefaultWeekdayCommand_Summary))]
		public async Task DefaultWeekdayCommand(DayOfWeek dag, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, $"dag {ScheduleUtil.GetStringFromDayOfWeek(dag)} ik", Context.Message, "default dag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("morgen", RunMode = RunMode.Sync), Summary("#" + nameof(Resources.DefaultScheduleModule_DefaultTomorrowCommand_Summary))]
		public async Task DefaultTomorrowCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "morgen ik", Context.Message, "default morgen");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("vandaag", RunMode = RunMode.Sync), Summary("#" + nameof(Resources.DefaultScheduleModule_DefaultTodayCommand_Summary))]
		public async Task DefaultTodayCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "vandaag ik", Context.Message, "default vandaag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("deze week", RunMode = RunMode.Async), Summary("#" + nameof(Resources.ScheduleModuleBase_ShowThisWeekWorkingDays_Summary))]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "deze week ik", Context.Message, "default deze week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("volgende week", RunMode = RunMode.Async), Summary("#" + nameof(Resources.ScheduleModuleBase_ShowNextWeekWorkingDays_Summary))]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "volgende week ik", Context.Message, "default volgende week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("over", RunMode = RunMode.Async), Summary("#" + nameof(Resources.ScheduleModuleBase_ShowNWeeksWorkingDays_Summary))]
		public async Task ShowNWeeksWorkingDaysCommand([Range(1, 52)] int aantal, [Name("eenheid (uur, dagen, of weken)")] string unit, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, $"over {aantal} {unit} ik", Context.Message, "default over");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("daarna", RunMode = RunMode.Sync), Summary("#" + nameof(Resources.ScheduleModuleBase_AfterCommand))]
		public async Task AfterCommand() {
			await ReplyAsync(Resources.DefaultScheduleModule_AfterCommand_BigProblem);
		}
	}
}
