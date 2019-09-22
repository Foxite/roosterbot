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
	[HiddenFromList]
	public class DefaultScheduleModule : RoosterModuleBase {
		[Priority(-10), Command("nu", RunMode = RunMode.Async)]
		public Task DefaultCurrentCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "nu ik", "default nu");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("hierna", RunMode = RunMode.Async)]
		public Task DefaultNextCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "hierna ik", "default hierna");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Async)]
		public Task DefaultWeekdayCommand(DayOfWeek dag, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, $"dag {dag.GetName(Culture)} ik", "default dag");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("morgen", RunMode = RunMode.Async)]
		public Task DefaultTomorrowCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "morgen ik", "default morgen");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("vandaag", RunMode = RunMode.Async)]
		public Task DefaultTodayCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "vandaag ik", "default vandaag");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("deze week", RunMode = RunMode.Async)]
		public Task ShowThisWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "deze week ik", "default deze week");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("volgende week", RunMode = RunMode.Async)]
		public Task ShowNextWeekWorkingDaysCommand([Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, "volgende week ik", "default volgende week");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		[Priority(-10), Command("over", RunMode = RunMode.Async)]
		public Task ShowNWeeksWorkingDaysCommand(int aantal, string unit, [Remainder] string wat = "") {
			if (string.IsNullOrWhiteSpace(wat)) {
				return Util.ExecuteSpecificCommand(CmdService, Context, $"over {aantal} {unit} ik", "default over");
			} else {
				ReplyErrorMessage(wat);
				return Task.CompletedTask;
			}
		}

		private void ReplyErrorMessage(string param) {
			if (MentionUtils.TryParseUser(param, out _)) {
				ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_MentionUserUnknown"));
			} else if (param == "IdentifierInfoReader_Self") {
				ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_UserUnknown"));
			} else {
				ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier"));
			}
		}
	}
}
