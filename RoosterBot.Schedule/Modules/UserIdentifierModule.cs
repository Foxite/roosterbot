using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	// TODO update ui strings, still sounds like it's restricted to student sets
	[Name("#UserIdentifierModule_ModuleName"), Description("#UserIdentifierModule_ModuleSummary"), Group("#UserIdentifierModule_Group")]
	public class UserIdentifierModule : RoosterModule {
		[Command("#UserIdentifierModule_CommandName"), Description("#UserIdentifierModule_CommandSummary"), RequirePrivate(false)]
		public async Task<CommandResult> UserClass([Name("#UserIdentifierModule_class_Name")] IdentifierInfo? newIdentifier = null) {
			if (newIdentifier == null) {
				IdentifierInfo? identifier = UserConfig.GetIdentifier();
				string response;
				if (identifier == null) {
					response = GetString("UserIdentifierModule_UserNotKnown");
				} else {
					response = GetString("UserIdentifierModule_UserIsInClass", identifier.DisplayText);
				}
				response += GetString("UserIdentifierModule_ChangeHint", ChannelConfig.CommandPrefix);
				return TextResult.Info(response);
			} else {
				if (newIdentifier.AssignableToUser) {
					IdentifierInfo? oldIdentifier = await UserConfig.SetIdentifierAsync(newIdentifier);
					if (oldIdentifier == null) {
						return TextResult.Success(GetString("UserIdentifierModule_ConfirmUserIsInClass", newIdentifier.DisplayText));
					} else if (oldIdentifier == newIdentifier) {
						return TextResult.Info(GetString("UserIdentifierModule_ConfirmNoChange", newIdentifier.DisplayText));
					} else {
						return TextResult.Success(GetString("UserIdentifierModule_ConfirmUserIsInClassWithOld", newIdentifier.DisplayText, oldIdentifier.DisplayText));
					}
				} else {
					return TextResult.Error("You can't assign that to yourself."); // TODO localize
				}
			}
		}
	}
}
