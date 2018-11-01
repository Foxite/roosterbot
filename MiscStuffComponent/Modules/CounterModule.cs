using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using MiscStuffComponent.Services;
using RoosterBot.Modules;

namespace MiscStuffComponent {
	[Group("counter")]
	public class CounterModule : EditableCmdModuleBase { // Does not use editable commands
		public CounterService Service { get; set; }

		[Command("")]
		public async Task GetCounterCommand(string counter) {
			try {
				string response = $"Dagen geleden dat {Service.GetCounterDescription(counter)}: {(int) (DateTime.Now - Service.GetDateCounter(counter)).TotalDays}";
				await ReplyAsync(response);
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError($"Uncaught exception {e.ToString()}");
			}
		}

		[Command("reset")]
		public async Task ResetCounterCommand(string counter) {
			try {
				Service.ResetDateCounter(counter);
				await ReplyAsync($"Dagen geleden dat {Service.GetCounterDescription(counter)}: 0");
			} catch (FileNotFoundException) {
				await MinorError("Die bestaat niet.");
			} catch (Exception e) {
				await FatalError($"Uncaught exception {e.ToString()}");
			}
		}
	}
}