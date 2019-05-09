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
		
		[Command("counter"), Priority(0), Summary("Kijk hoever een couter is.")]
		public async Task GetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: {Service.FormatTimeSpan(timeSinceReset)}.\n";
				response += $"(Laatst gereset op {counterData.LastResetDate.ToShortDateString()} om {counterData.LastResetDate.ToShortTimeString()}.)\n";
				response += $"De highscore is {Service.FormatTimeSpan(counterData.HighScoreTimespan)}.\n";
				response += $"Reset de counter met \"!counter reset {counter}\".";
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}

		[Command("counter reset"), Alias("reset counter"), Priority(1), Summary("Zet een counter weer op nul.")]
		public async Task ResetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: 0 (was {Service.FormatTimeSpan(timeSinceReset)}).\n";
				bool newRecord = Service.ResetDateCounter(counter);
				if (newRecord) {
					response += $"Dat is een nieuw record! De vorige highscore was {Service.FormatTimeSpan(counterData.HighScoreTimespan)}.";
				} else {
					response += $"De highscore is {Service.FormatTimeSpan(counterData.HighScoreTimespan)}.";
				}
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError("Uncaught exception", e);
			}
		}
	}
}
