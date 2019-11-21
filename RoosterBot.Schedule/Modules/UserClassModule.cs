using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LocalizedModule("nl-NL", "en-US"), Name("#UserClassModule_Name"), Summary("#UserClassModule_Summary"), Group("#UserClassModule_Group")]
	public class UserClassModule : RoosterModuleBase {
		[Command("#UserClassModule_GetClassForUser", RunMode = RunMode.Async), Summary("#UserClassModule_GetClassForUser_Summary")]
		public Task GetClassForUser() {
			StudentSetInfo? ssi = UserConfig.GetStudentSet();
			string response;
			if (ssi == null) {
				response = GetString("UserClassModule_GetClassForUser_UserNotKnown");
			} else {
				response = GetString("UserClassModule_GetClassForUser_UserIsInClass", ssi.DisplayText);
			}
			response += GetString("UserClassModule_GetClassForUser_ChangeHint", GuildConfig.CommandPrefix);
			ReplyDeferred(response);
			return Task.CompletedTask;
		}

		[Command("#UserClassModule_SetClassForUser", RunMode = RunMode.Async), Summary("#UserClassModule_SetClassForUser_Summary"), RequireContext(ContextType.Guild, ErrorMessage = "Deze command moet binnen een server worden gebruikt.")]
		public async Task SetClassForUser([Name("#UserClassModule_SetClassForUser_class_Name")] StudentSetInfo newStudentSet) {
			StudentSetInfo? oldStudentSet = await UserConfig.SetStudentSetAsync(newStudentSet);
			if (oldStudentSet == null) {
				ReplyDeferred(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClass", newStudentSet.DisplayText));
			} else if (oldStudentSet == newStudentSet) {
				ReplyDeferred(GetString("UserClassModule_SetClassForUser_ConfirmNoChange", newStudentSet.DisplayText));
			} else {
				ReplyDeferred(GetString("UserClassModule_SetClassForUser_ConfirmUserIsInClassWithOld", newStudentSet.DisplayText, oldStudentSet.DisplayText));
			}
		}
	}
}
