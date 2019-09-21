using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	[Name("#CounterModule_Name")]
	public class CounterModule : RoosterModuleBase {
		public CounterService Service { get; set; }
		
		[Command("counter"), Priority(0), Summary("#CounterModule_GetCounterCommand_Summary")]
		public Task GetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = GetString("CounterModule_GetCounterCommand_FullText",
												Service.GetCounterDescription(counter),
												FormatTimeSpan(timeSinceReset),
												counterData.LastResetDate.ToString("dd-MM-yyyy"),
												counterData.LastResetDate.ToString("HH:mm"),
												FormatTimeSpan(counterData.HighScoreTimespan));
				ReplyDeferred(response);
				return Task.CompletedTask;
			} catch (FileNotFoundException) {
				return MinorError(GetString("CounterModule_GetCounterCommand_CounterDoesNotExist"));
			} catch (ArgumentException e) {
				return FatalError("Invalid counter", e);
			} catch (Exception e) {
				return FatalError("Uncaught exception", e);
			}
		}

		[Command("counter reset"), Alias("reset counter"), Priority(1), Summary("#CounterModule_ResetCounterCommand_Summary")]
		public Task ResetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				
				string counterDescription = Service.GetCounterDescription(counter);
				string previousTimespan = FormatTimeSpan(timeSinceReset);
				string previousHighscore = FormatTimeSpan(counterData.HighScoreTimespan);

				bool newRecord = Service.ResetDateCounter(counter);

				string response = string.Format(newRecord ? GetString("CounterModule_ResetCounterCommand_ResponseNewHighscore")
														  : GetString("CounterModule_ResetCounterCommand_ResponseNoNewHighscore"),
												counterDescription, previousTimespan, previousHighscore);
				ReplyDeferred(response);
				return Task.CompletedTask;
			} catch (FileNotFoundException) {
				return MinorError(GetString("CounterModule_GetCounterCommand_CounterDoesNotExist"));
			} catch (Exception e) {
				return FatalError("Uncaught exception", e);
			}
		}

		public string FormatTimeSpan(TimeSpan ts) {
			string days;
			if (((long) ts.TotalDays) == 1) {
				days = GetString("CounterService_FormatTimeSpan_DaysSingular", (long) ts.TotalDays);
			} else {
				days = GetString("CounterService_FormatTimeSpan_DaysPlural", (long) ts.TotalDays);
			}

			string separator = GetString("CounterService_FormatTimeSpan_Separator");

			string hours;
			if (((long) ts.Hours) == 1) {
				hours = GetString("CounterService_FormatTimeSpan_HoursSingular", (long) ts.Hours);
			} else {
				hours = GetString("CounterService_FormatTimeSpan_HoursPlural", (long) ts.Hours);
			}

			return GetString("CounterService_FormatTimeSpan_Result", days, separator, hours);
		}
	}
}
