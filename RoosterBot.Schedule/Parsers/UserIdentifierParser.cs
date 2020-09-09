using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class UserIdentifierParser : RoosterTypeParser<IdentifierInfo> {
		public override string TypeDisplayName => "#UserIdentifierParser_TypeDisplayName";

		public async override ValueTask<RoosterTypeParserResult<IdentifierInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			bool byMention;
			IdentifierInfo? result;

			if (context.ServiceProvider.GetRequiredService<ResourceService>().GetString(context.Culture, "UserIdentifierParser_Self").Split("|").Contains(input.ToLower())) {
				result = context.UserConfig.GetIdentifier();
				byMention = false;
			} else {
				RoosterTypeParserResult<IUser> userResult = await context.ServiceProvider.GetRequiredService<RoosterCommandService>().GetPlatformSpecificParser<IUser>().ParseAsync(parameter, input, context);
				if (userResult.IsSuccessful) {
					result = (await context.ServiceProvider.GetRequiredService<UserConfigService>().GetConfigAsync(userResult.Value.GetReference())).GetIdentifier();
					byMention = true;
				} else {
					return Unsuccessful(false, userResult.Reason);
				}
			}

			if (result is null) {
				string message;
				if (byMention) {
					message = "#UserIdentifierParser_CheckFailed_MentionUser";
				} else {
					message = "#UserIdentifierParser_CheckFailed_MentionSelf";
				}
				return Unsuccessful(true, message, context.ChannelConfig.CommandPrefix);
			} else {
				return Successful(result);
			}
		}
	}
}
