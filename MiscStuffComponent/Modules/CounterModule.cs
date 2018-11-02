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
				DateTime counterDT = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.Now - counterDT;
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: {(int) timeSinceReset.TotalDays} dagen {timeSinceReset.Hours} uur\n";
				response += $"(Laatst gereset op {counterDT.ToShortDateString()} om {counterDT.ToShortTimeString()})\n";
				response += $"Reset de counter met \"!counter reset {counter}\"";
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
				DateTime counterDT = Service.GetDateCounter(counter);
				TimeSpan timeSinceReset = DateTime.Now - counterDT;
				await ReplyAsync($"Dagen geleden dat {Service.GetCounterDescription(counter)}: 0 (was {(int) timeSinceReset.TotalDays} dagen {timeSinceReset.Hours} uur)");
				Service.ResetDateCounter(counter);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError($"Uncaught exception {e.ToString()}");
			}
		}
	}
}