using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("UserClassModule"), Name("Jouw klas"), Summary("Met deze commands kun je instellen in welke klas je zit, zodat je bij Rooster commands niets hoeft in te vullen.")]
	public class UserClassModule : EditableCmdModuleBase {
		public UserClassesService Classes { get; set; }
		public UserClassRoleService Roles { get; set; }

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

		[Command("ik"), Summary("Stel in in welke klas jij zit."), RequireContext(ContextType.Guild, ErrorMessage = "Deze command moet binnen een server worden gebruikt.")]
		public async Task SetClassForUser([Name("jouw klas")] string studentSet) {
			StudentSetInfo oldStudentSet = await Classes.GetClassForDiscordUser(Context.User);
			string clazzName = studentSet.ToUpper();

			try {
				await Classes.SetClassForDiscordUser(Context.User, clazzName);
			} catch (ArgumentException) {
				await ReplyAsync("Dat is geen klas.");
				return;
			}

			await ReplyAsync("Genoteerd: jij zit in " + clazzName + ".");

			StudentSetInfo newStudentSet = new StudentSetInfo() { ClassName = clazzName };

			IGuildUser guildUser = Context.User as IGuildUser;

			try {
				IRole[] oldRoles;
				if (oldStudentSet == null) {
					oldRoles = new[] { Context.Guild.GetRole(278937741478330389u) };
				} else {
					oldRoles = Roles.GetRolesForStudentSet(Context.Guild, oldStudentSet).ToArray();
				}
				IRole[] newRoles = Roles.GetRolesForStudentSet(Context.Guild, newStudentSet).ToArray();

				await guildUser.RemoveRolesAsync(oldRoles);
				await guildUser.AddRolesAsync(newRoles);
			} catch (Exception e) {
				Logger.Error("UserClassModule", "Failed to assign role", e);
				await Config.BotOwner.SendMessageAsync("Failed to assign role: " + e.ToString());
			}
		}
	}
}
