using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// This class has 3 purposes:
	// - Provide a single set of commands for the 3 separate modules
	// - Make it possible for a missing parameter to be resolved to "ik"
	// - Provide a more useful error message in case a parameter is not understood
	// 
	// The first one is obsolete since there is now only one module for the commands.
	// The second one can conceivably be accomplished by providing a default value, however, this is hard because default parameter types have to be compile-time constants.
	// The third one is still valid, but in theory, there needs to be a way to do this without having a separate module.
	// 
	// None of these necessities are the result of writing good code, so there is a long-standing todo item:
	// TODO: make this class obsolete.
	[LogTag("DefaultCommandsModule"), Name("#DefaultScheduleModule_Name"),
		Summary("#DefaultScheduleModule_Summary"),
		Remarks("#DefaultScheduleModule_Remarks")]
	public class DefaultScheduleModule : EditableCmdModuleBase {
		[Priority(-10), Command("nu", RunMode = RunMode.Async), Alias("rooster"), Summary("#DefaultScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task DefaultCurrentCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "nu ik", Context.Message, "default nu");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("#DefaultScheduleModule_DefaultNextCommand_Summary")]
		public async Task DefaultNextCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "hierna ik", Context.Message, "default hierna");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task DefaultWeekdayCommand(DayOfWeek dag, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, $"dag {ScheduleUtil.GetStringFromDayOfWeek(Culture, dag)} ik", Context.Message, "default dag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("morgen", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task DefaultTomorrowCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "morgen ik", Context.Message, "default morgen");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("vandaag", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultTodayCommand_Summary")]
		public async Task DefaultTodayCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "vandaag ik", Context.Message, "default vandaag");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("deze week", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowThisWeekWorkingDays_Summary")]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "deze week ik", Context.Message, "default deze week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("volgende week", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowNextWeekWorkingDays_Summary")]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "volgende week ik", Context.Message, "default volgende week");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("over", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowNWeeksWorkingDays_Summary")]
		public async Task ShowNWeeksWorkingDaysCommand(int aantal, [Name("eenheid (uur, dagen, of weken)")] string unit, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, $"over {aantal} {unit} ik", Context.Message, "default over");
			} else {
				await ReplyErrorMessage(wat);
			}
		}

		[Priority(-10), Command("daarna", RunMode = RunMode.Sync), Summary("#ScheduleModuleBase_AfterCommand")]
		public async Task AfterCommand() {
			await ReplyAsync(ResourcesService.GetString(Culture, "DefaultScheduleModule_AfterCommand_BigProblem"));
		}

		private async Task ReplyErrorMessage(string param) {
			if (MentionUtils.TryParseUser(param, out _)) {
				await ReplyAsync(ResourcesService.GetString(Culture, "DefaultScheduleModule_ReplyErrorMessage_MentionUserUnknown"));
			} else if (param == "ik") {
				await ReplyAsync(ResourcesService.GetString(Culture, "DefaultScheduleModule_ReplyErrorMessage_UserUnknown"));
			} else {
				await ReplyAsync(ResourcesService.GetString(Culture, "DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier"));
			}
		}
	}
}
