using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("UserClassModule"), Name("#" + nameof(Resources.UserClassModule_Name)), Summary("#" + nameof(Resources.UserClassModule_Summary))]
	public class UserClassModule : EditableCmdModuleBase {
		public IUserClassesService Classes { get; set; }
		public IdentifierValidationService Validation { get; set; }

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
			StudentSetInfo studentSet = Validation.Validate<StudentSetInfo>(Context, clazz);
			if (studentSet != null) {
				await Classes.SetClassForDiscordUserAsync(Context, Context.User, studentSet);
				await ReplyAsync(string.Format(Resources.UserClassModule_SetClassForUser_ConfirmUserIsInClass, studentSet.DisplayText));

			} else {
				await ReplyAsync(Resources.UserClassModule_SetClassForUser_InvalidClass);
			}
		}
	}
}
