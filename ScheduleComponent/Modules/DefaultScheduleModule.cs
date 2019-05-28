using System.Threading.Tasks;
using Discord.Commands;
using ScheduleComponent.Services;
using RoosterBot.Modules;
using RoosterBot.Attributes;

namespace ScheduleComponent.Modules {
	// Provides several "virtual" commands that don't do anything useful, but serve as a single list item for 3 different "versions" of each command
	// They do provide an error message in case they are invoked and no TypeReader could figure out which version was to be used.
	[LogTag("DefaultCommandsModule"), Name("Rooster"),
		Summary("Begrijpt automatisch of je een klas, leraar, of lokaal bedoelt."),
		Remarks("Met `!ik` kun je instellen in welke klas jij zit, zodat je hier niets hoeft in te vullen.")]
	public class DefaultScheduleModule : EditableCmdModuleBase {
		private const string ErrorMessage = "Ik weet niet of je het over een leraar, klas of lokaal hebt.";

		public CommandMatchingService MatchingService { get; set; }

		[Priority(-10), Command("nu", RunMode = RunMode.Sync), Alias("rooster"), Summary("Kijk wat er nu op het rooster staat.")]
		public async Task DefaultCurrentCommand([Remainder] string wat = "") {
			await ReplyAsync(ErrorMessage);
		}

		[Priority(-10), Command("hierna", RunMode = RunMode.Sync), Alias("later", "straks", "zometeen"), Summary("Kijk wat er hierna op het rooster staat")]
		public async Task DefaultNextCommand([Remainder] string wat = "") {
			await ReplyAsync(ErrorMessage);
		}

		[Priority(-10), Command("dag", RunMode = RunMode.Sync), Summary("Het rooster voor een bepaalde dag. Als je de huidige dag gebruikt, pak ik volgende week. `!vandaag` doet dit niet.")]
		public async Task DefaultWeekdayCommand([Remainder] string wat_en_weekdag) {
			await ReplyAsync("Ik weet niet of je het over een leraar, klas of lokaal hebt, en/of ik begrijp niet welke weekdag je bedoelt.");
		}

		[Priority(-10), Command("morgen", RunMode = RunMode.Sync), Summary("Wat er morgen op het rooster staat.")]
		public async Task DefaultTomorrowCommand([Remainder] string wat) {
			await ReplyAsync(ErrorMessage);
		}

		[Priority(-10), Command("vandaag", RunMode = RunMode.Sync), Summary("Wat er vandaag op het rooster staat.")]
		public async Task DefaultTodayCommand([Remainder] string wat) {
			await ReplyAsync(ErrorMessage);
		}

		[Priority(-10), Command("daarna", RunMode = RunMode.Sync), Summary("Kijk wat er gebeurt na het laatste wat je hebt bekeken.")]
		public async Task DefaultAfterCommand() {
			await ReplyAsync("Als je dit ziet is er een groot probleem");
		}
	}
}
