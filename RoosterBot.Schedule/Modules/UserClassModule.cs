using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("UserClassModule"), Name("#UserClassModule_Name"), Summary("#UserClassModule_Summary")]
	public class UserClassModule : RoosterModuleBase {
		public IUserClassesService Classes { get; set; }
		public IdentifierValidationService Validation { get; set; }

		[Command("ik"), Summary("#UserClassModule_GetClassForUser_Summary")]
		public async Task GetClassForUser() {
			string clazz = (await Classes.GetClassForDiscordUserAsync(Context, Context.User))?.DisplayText;
			string response;
			if (clazz == null) {
				response = GetString("UserClassModule_GetClassForUser_UserNotKnown");
			} else {
				response = GetString("UserClassModule_GetClassForUser_UserIsInClass", clazz);
			}
			response += GetString("UserClassModule_GetClassForUser_ChangeHint");
			await ReplyAsync(response);
		}
		
		[Command("ik"), Summary("#UserClassModule_SetClassForUser_Summary")]
		public async Task SetClassForUser([Name("#UserClassModule_SetClassForUser_class_Name")] string clazzName) {
			StudentSetInfo newStudentSet = await Validation.ValidateAsync<StudentSetInfo>(Context, clazzName);
			if (newStudentSet != null) {
				//await Classes.GetClassForDiscordUserAsync(Context, Context.User);
				StudentSetInfo oldStudentSet = await Classes.SetClassForDiscordUserAsync(Context, Context.User, newStudentSet);
				if (oldStudentSet == null) {
					await ReplyAsync(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClass", newStudentSet.DisplayText));
				} else {
					await ReplyAsync(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClassWithOld", newStudentSet.DisplayText, oldStudentSet.DisplayText));
				}
			} else {
				await ReplyAsync(GetString("UserClassModule_SetClassForUser_InvalidClass"));
			}
		}
	}
}
