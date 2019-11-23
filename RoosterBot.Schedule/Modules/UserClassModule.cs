using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	[LocalizedModule("nl-NL", "en-US"), Name("#UserClassModule_ModuleName"), Description("#UserClassModule_ModuleSummary"), Group("#UserClassModule_Group")]
	public class UserClassModule : RoosterModuleBase {
		[Command("#UserClassModule_CommandName"), RunMode(RunMode.Parallel), Description("#UserClassModule_CommandSummary"), RequireContext(ContextType.Guild)]
		public async Task SetClassForUser([Name("#UserClassModule_class_Name")] StudentSetInfo? newStudentSet = null) {
			if (newStudentSet == null) {
				StudentSetInfo? ssi = UserConfig.GetStudentSet();
				string response;
				if (ssi == null) {
					response = GetString("UserClassModule_UserNotKnown");
				} else {
					response = GetString("UserClassModule_UserIsInClass", ssi.DisplayText);
				}
				response += GetString("UserClassModule_ChangeHint", GuildConfig.CommandPrefix);
				ReplyDeferred(response);
			} else {
				StudentSetInfo? oldStudentSet = await UserConfig.SetStudentSetAsync(newStudentSet);
				if (oldStudentSet == null) {
					ReplyDeferred(GetString("UserClassModule_ConfirmUserIsInClass", newStudentSet.DisplayText));
				} else if (oldStudentSet == newStudentSet) {
					ReplyDeferred(GetString("UserClassModule_ConfirmNoChange", newStudentSet.DisplayText));
				} else {
					ReplyDeferred(GetString("UserClassModule_ConfirmUserIsInClassWithOld", newStudentSet.DisplayText, oldStudentSet.DisplayText));
				}
			}
		}
	}
}
