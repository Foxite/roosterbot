using System;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("UserClassModule"), Name("Jouw klas"), Summary("Met deze commands kun je instellen in welke klas je zit, zodat je bij Rooster commands niets hoeft in te vullen.")]
	public class UserClassModule : EditableCmdModuleBase {
		public UserClassesService Classes { get; set; }

		[Command("ik"), Summary("Kijk in welke klas jij zit, volgens mijn gegevens.")]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUser(Context.User))?.DisplayText;
			string response;
			if (clazz == null) {
				response = "Ik weet niet in welke klas jij zit.";
			} else {
				response = $"Jij zit in {clazz}.";
			}
			response += "Gebruik bijvoorbeeld `!ik <jouw klas>` om dit te veranderen.";
			await ReplyAsync(response);
		}
		
		[Command("ik"), Summary("Stel in in welke klas jij zit.")]
		public async Task SetClassForUser([Name("jouw klas")] string clazz) {
			try {
				await Classes.SetClassForDiscordUser(Context.User, clazz.ToUpper());
				await ReplyAsync("Genoteerd: jij zit in " + clazz.ToUpper() + ".");
			} catch (ArgumentException) {
				await ReplyAsync("Dat is geen klas.");
			}
		}
	}
}
