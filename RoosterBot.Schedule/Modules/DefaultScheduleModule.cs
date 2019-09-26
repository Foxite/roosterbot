using System;
using System.Linq;
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
		[Alias("hierna", "morgen", "vandaag", "deze week", "volgende week")]
		public Task ShowNextWeekWorkingDaysCommand([Remainder] string wat = "") {
			return ExecuteDefaultCommand(wat);
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Async)]
		public Task DefaultWeekdayCommand(DayOfWeek dag, [Remainder] string wat = "") {
			return ExecuteDefaultCommand(dag.GetName(Culture), wat);
		}

		[Priority(-10), Command("over", RunMode = RunMode.Async)]
		public Task ShowNWeeksWorkingDaysCommand(int aantal, string unit, [Remainder] string wat = "") {
			return ExecuteDefaultCommand(aantal.ToString(), unit, wat);
		}

		private Task ExecuteDefaultCommand(params string[] parameters) {
			string last = parameters.Last();
			if (string.IsNullOrWhiteSpace(last)) {
				string message = Context.Message.Content;
				if (message.StartsWith(Config.CommandPrefix)) {
					message = message.Substring(Config.CommandPrefix.Length);
				}
				return Util.ExecuteSpecificCommand(CmdService, Context, message + " ik", "default schedule");
			} else {
				if (MentionUtils.TryParseUser(last, out _)) {
					ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_MentionUserUnknown"));
				} else if (last == GetString("IdentifierInfoReader_Self")) {
					ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_UserUnknown"));
				} else {
					ReplyDeferred(GetString("DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier"));
				}
				return Task.CompletedTask;
			}
		}
	}
}
