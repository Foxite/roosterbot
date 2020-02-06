using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	// TODO update resource names
	// TODO update ui strings, still sounds like it's restricted to student sets
	[Name("#UserClassModule_ModuleName"), Description("#UserClassModule_ModuleSummary"), Group("#UserClassModule_Group")]
	public class UserIdentifierModule : RoosterModule {
		[Command("#UserClassModule_CommandName"), Description("#UserClassModule_CommandSummary"), RequirePrivate(false)]
		public async Task<CommandResult> UserClass([Name("#UserClassModule_class_Name")] IdentifierInfo? newIdentifier = null) {
			if (newIdentifier == null) {
				IdentifierInfo? identifier = UserConfig.GetIdentifier();
				string response;
				if (identifier == null) {
					response = GetString("UserClassModule_UserNotKnown");
				} else {
					response = GetString("UserClassModule_UserIsInClass", identifier.DisplayText);
				}
				response += GetString("UserClassModule_ChangeHint", ChannelConfig.CommandPrefix);
				return TextResult.Info(response);
			} else {
				if (newIdentifier.AssignableToUser) {
					IdentifierInfo? oldIdentifier = await UserConfig.SetIdentifierAsync(newIdentifier);
					if (oldIdentifier == null) {
						return TextResult.Success(GetString("UserClassModule_ConfirmUserIsInClass", newIdentifier.DisplayText));
					} else if (oldIdentifier == newIdentifier) {
						return TextResult.Info(GetString("UserClassModule_ConfirmNoChange", newIdentifier.DisplayText));
					} else {
						return TextResult.Success(GetString("UserClassModule_ConfirmUserIsInClassWithOld", newIdentifier.DisplayText, oldIdentifier.DisplayText));
					}
				} else {
					return TextResult.Error("You can't assign that to yourself."); // TODO localize
				}
			}
		}
	}
}
