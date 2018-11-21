using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using MiscStuffComponent.Services;
using RoosterBot.Modules;

namespace MiscStuffComponent.Modules {
	[Group("counter"), Alias("teller"), RoosterBot.Attributes.LogTag("CoM")]
	public class CounterModule : EditableCmdModuleBase { // Does not use editable commands
		public CounterService Service { get; set; }
		
		[Command(""), Priority(0)]
		public async Task GetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: {(long) timeSinceReset.TotalDays} dagen {timeSinceReset.Hours} uur.\n";
				response += $"(Laatst gereset op {counterData.LastResetDate.ToShortDateString()} om {counterData.LastResetDate.ToShortTimeString()}.)\n";
				response += $"De highscore is {(long) counterData.HighScoreTimespan.TotalDays} dagen en {counterData.HighScoreTimespan.Hours} uur.\n";
				response += $"Reset de counter met \"!counter reset {counter}\".";
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError($"Uncaught exception {e.ToString()}");
			}
		}

		[Command("reset"), Priority(1)]
		public async Task ResetCounterCommand([Remainder] string counter) {
			try {
				CounterData counterData = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.UtcNow - counterData.LastResetDate;
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: 0 (was {(long) timeSinceReset.TotalDays} dagen {timeSinceReset.Hours} uur).\n";
				bool newRecord = Service.ResetDateCounter(counter);
				if (newRecord) {
					response += $"Dat is een nieuw record! De vorige highscore was {(long) counterData.HighScoreTimespan.TotalDays} dagen en {counterData.HighScoreTimespan.Hours} uur.";
				} else {
					response += $"De highscore is {(long) counterData.HighScoreTimespan.TotalDays} dagen en {counterData.HighScoreTimespan.Hours} uur.";
				}
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError($"Uncaught exception {e.ToString()}");
			}
		}
	}
}