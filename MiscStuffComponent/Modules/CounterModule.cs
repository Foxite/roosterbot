using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using MiscStuffComponent.Services;
using RoosterBot.Modules;
using RoosterBot.Attributes;

namespace MiscStuffComponent.Modules {
	[LogTag("CounterModule"), Name("Counters")]
	public class CounterModule : EditableCmdModuleBase { // Does not use editable commands
		public CounterService Service { get; set; }
		
		[Command("counter"), Priority(0), Summary("Kijk hoever een counter is.")]
		public async Task GetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = string.Format(Resources.CounterModule_GetCounterCommand_FullText,
												Service.GetCounterDescription(counter),
												Service.FormatTimeSpan(timeSinceReset),
												counterData.LastResetDate.ToString("dd-MM-yyyy"),
												counterData.LastResetDate.ToString("HH:mm"),
												Service.FormatTimeSpan(counterData.HighScoreTimespan));
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError(Resources.CounterModule_GetCounterCommand_CounterDoesNotExist);
			} catch (ArgumentException e) {
				await FatalError("Invalid counter", e);
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}

		[Command("counter reset"), Alias("reset counter"), Priority(1), Summary("Zet een counter weer op nul.")]
		public async Task ResetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				
				string counterDescription = Service.GetCounterDescription(counter);
				string previousTimespan = Service.FormatTimeSpan(timeSinceReset);
				string previousHighscore = Service.FormatTimeSpan(counterData.HighScoreTimespan);

				bool newRecord = Service.ResetDateCounter(counter);

				string response = string.Format(newRecord ? Resources.CounterModule_ResetCounterCommand_ResponseNewHighscore
														  : Resources.CounterModule_ResetCounterCommand_ResponseNoNewHighscore,
												counterDescription, previousTimespan, previousHighscore);
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError(Resources.CounterModule_GetCounterCommand_CounterDoesNotExist);
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}
	}
}
