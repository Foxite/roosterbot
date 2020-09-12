using Qmmands;

namespace RoosterBot.Schedule {
	[Name("#UserIdentifierModule_ModuleName"), Description("#UserIdentifierModule_ModuleSummary"), Group("#UserIdentifierModule_Group")]
	public class UserIdentifierModule : RoosterModule {
		[Command("#UserIdentifierModule_GetCommandName"), Description("#UserIdentifierModule_GetCommandSummary"), RequirePrivate(false)]
		public CommandResult GetIdentifier() {
			IdentifierInfo? identifier = UserConfig.GetIdentifier();
			string response;
			if (identifier == null) {
				response = GetString("UserIdentifierModule_UserNotKnown");
			} else {
				response = GetString("UserIdentifierModule_UserIsIdentifier", identifier.DisplayText);
			}
			response += GetString("UserIdentifierModule_ChangeHint", ChannelConfig.CommandPrefix);
			return TextResult.Info(response);
		}

		[Command("#UserIdentifierModule_SetCommandName"), Description("#UserIdentifierModule_SetCommandSummary"), RequirePrivate(false)]
		public CommandResult SetIdentifier([Name("#UserIdentifierModule_class_Name")] IdentifierInfo newIdentifier) {
			if (newIdentifier.AssignableToUser) {
				IdentifierInfo? oldIdentifier = UserConfig.SetIdentifier(newIdentifier);
				if (oldIdentifier == null) {
					return TextResult.Success(GetString("UserIdentifierModule_ConfirmUserIsIdentifier", newIdentifier.DisplayText));
				} else if (oldIdentifier == newIdentifier) {
					return TextResult.Info(GetString("UserIdentifierModule_ConfirmNoChange", newIdentifier.DisplayText));
				} else {
					return TextResult.Success(GetString("UserIdentifierModule_ConfirmUserIsIdentifierWithOld", newIdentifier.DisplayText, oldIdentifier.DisplayText));
				}
			} else {
				return TextResult.Error(GetString("UserIdentifierModule_NotAssignableToUser"));
			}
		}
	}
}
