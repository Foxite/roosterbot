using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	[Name("#UserClassModule_ModuleName"), Description("#UserClassModule_ModuleSummary"), Group("#UserClassModule_Group")]
	public class UserClassModule : RoosterModule {
		[Command("#UserClassModule_CommandName"), Description("#UserClassModule_CommandSummary"), RequireContext(ContextType.Guild)]
		public async Task<CommandResult> UserClass([Name("#UserClassModule_class_Name")] StudentSetInfo? newStudentSet = null) {
			if (newStudentSet == null) {
				StudentSetInfo? ssi = UserConfig.GetStudentSet();
				string response;
				if (ssi == null) {
					response = GetString("UserClassModule_UserNotKnown");
				} else {
					response = GetString("UserClassModule_UserIsInClass", ssi.DisplayText);
				}
				response += GetString("UserClassModule_ChangeHint", GuildConfig.CommandPrefix);
				return TextResult.Info(response);
			} else {
				StudentSetInfo? oldStudentSet = await UserConfig.SetStudentSetAsync(newStudentSet);
				if (oldStudentSet == null) {
					return TextResult.Success(GetString("UserClassModule_ConfirmUserIsInClass", newStudentSet.DisplayText));
				} else if (oldStudentSet == newStudentSet) {
					return TextResult.Info(GetString("UserClassModule_ConfirmNoChange", newStudentSet.DisplayText));
				} else {
					return TextResult.Success(GetString("UserClassModule_ConfirmUserIsInClassWithOld", newStudentSet.DisplayText, oldStudentSet.DisplayText));
				}
			}
		}
	}
}
