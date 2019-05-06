using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("UserClassTest")]
	public class TestUserClassModule : EditableCmdModuleBase {
		public UserClassesService Classes { get; set; }

		[Command("getclass")]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUser(Context.User)).DisplayText;
			await ReplyAsync(clazz);
		}
		
		[Command("setclass")]
		public async Task SetClassForUser(string clazz) {
			await Classes.SetClassForDiscordUser(Context.User, clazz);
			await ReplyAsync("OK: " + clazz);
		}
	}
}
