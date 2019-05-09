using System;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("UserClassTest"), Name("Jouw klas"), Summary("Met deze commands kun je instellen in welke klas je zit, zodat je bij Rooster commands niets hoeft in te vullen.")]
	public class UserClassModule : EditableCmdModuleBase {
		public UserClassesService Classes { get; set; }

		[Command("ik")]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUser(Context.User)).DisplayText;
			await ReplyAsync("Jij zit in " + clazz + ". Gebruik bijvoorbeeld `!ik 2gd1` om dit te veranderen.");
		}
		
		[Command("ik")]
		public async Task SetClassForUser([Summary("In welke klas je zit."), Name("klas")] string clazz) {
			try {
				await Classes.SetClassForDiscordUser(Context.User, clazz.ToUpper());
				await ReplyAsync("Genoteerd: jij zit in " + clazz.ToUpper() + ".");
			} catch (ArgumentException) {
				await ReplyAsync("Dat is geen klas.");
			}
		}
	}
}
