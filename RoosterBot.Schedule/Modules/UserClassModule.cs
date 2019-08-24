using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("UserClassModule"), Name("#" + nameof(Resources.UserClassModule_Name)), Summary("#" + nameof(Resources.UserClassModule_Summary))]
	public class UserClassModule : EditableCmdModuleBase {
		public IUserClassesService Classes { get; set; }

		[Command("ik"), Summary("#" + nameof(Resources.UserClassModule_GetClassForUser_Summary))]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUserAsync(Context, Context.User))?.DisplayText;
			string response;
			if (clazz == null) {
				response = Resources.UserClassModule_GetClassForUser_UserNotKnown;
			} else {
				response = string.Format(Resources.UserClassModule_GetClassForUser_UserIsInClass, clazz);
			}
			response += Resources.UserClassModule_GetClassForUser_ChangeHint;
			await ReplyAsync(response);
		}
		
		[Command("ik"), Summary("#" + nameof(Resources.UserClassModule_SetClassForUser_Summary))]
		public async Task SetClassForUser([Name("#" + nameof(Resources.UserClassModule_SetClassForUser_class_Name))] string clazz) {
			try {
				await Classes.SetClassForDiscordUserAsync(Context, Context.User, clazz.ToUpper());
				await ReplyAsync(string.Format(Resources.UserClassModule_SetClassForUser_ConfirmUserIsInClass, clazz.ToUpper()));
			} catch (ArgumentException) {
				await ReplyAsync(Resources.UserClassModule_SetClassForUser_InvalidClass);
			}
		}
	}
}
