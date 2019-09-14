using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	[LogTag("CounterModule"), Name("#CounterModule_Name")]
	public class CounterModule : EditableCmdModuleBase { // Does not use editable commands
		public CounterService Service { get; set; }
		
		[Command("counter"), Priority(0), Summary("#CounterModule_GetCounterCommand_Summary")]
		public async Task GetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = string.Format(ResourcesService.GetString(Culture, "CounterModule_GetCounterCommand_FullText"),
												Service.GetCounterDescription(counter),
												FormatTimeSpan(timeSinceReset),
												counterData.LastResetDate.ToString("dd-MM-yyyy"),
												counterData.LastResetDate.ToString("HH:mm"),
												FormatTimeSpan(counterData.HighScoreTimespan));
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError(ResourcesService.GetString(Culture, "CounterModule_GetCounterCommand_CounterDoesNotExist"));
			} catch (ArgumentException e) {
				await FatalError("Invalid counter", e);
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}

		[Command("counter reset"), Alias("reset counter"), Priority(1), Summary("#CounterModule_ResetCounterCommand_Summary")]
		public async Task ResetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				
				string counterDescription = Service.GetCounterDescription(counter);
				string previousTimespan = FormatTimeSpan(timeSinceReset);
				string previousHighscore = FormatTimeSpan(counterData.HighScoreTimespan);

				bool newRecord = Service.ResetDateCounter(counter);

				string response = string.Format(newRecord ? ResourcesService.GetString(Culture, "CounterModule_ResetCounterCommand_ResponseNewHighscore")
														  : ResourcesService.GetString(Culture, "CounterModule_ResetCounterCommand_ResponseNoNewHighscore"),
												counterDescription, previousTimespan, previousHighscore);
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError(ResourcesService.GetString(Culture, "CounterModule_GetCounterCommand_CounterDoesNotExist"));
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}

		public string FormatTimeSpan(TimeSpan ts) {
			string days;
			if (((long) ts.TotalDays) == 1) {
				days = string.Format(ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_DaysSingular"), (long) ts.TotalDays);
			} else {
				days = string.Format(ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_DaysPlural"), (long) ts.TotalDays);
			}

			string separator = ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_Separator");

			string hours;
			if (((long) ts.Hours) == 1) {
				hours = string.Format(ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_HoursSingular"), (long) ts.Hours);
			} else {
				hours = string.Format(ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_HoursPlural"), (long) ts.Hours);
			}

			return string.Format(ResourcesService.GetString(Culture, "CounterService_FormatTimeSpan_Result"), days, separator, hours);
		}
	}
}
