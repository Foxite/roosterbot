using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class StudentSetInfoParser : IdentifierInfoParserBase<StudentSetInfo> {
		public override string TypeDisplayName => "#StudentSetInfo_TypeDisplayName";

		public async override ValueTask<RoosterTypeParserResult<StudentSetInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			RoosterTypeParserResult<StudentSetInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				bool byMention;
				StudentSetInfo? result;

				if (input.ToLower() == context.ServiceProvider.GetRequiredService<ResourceService>().GetString(context.Culture, "IdentifierInfoReader_Self")) {
					result = context.UserConfig.GetStudentSet();
					byMention = false;
				} else {
					RoosterTypeParserResult<IUser> userResult = await context.ServiceProvider.GetRequiredService<RoosterCommandService>().GetPlatformSpecificParser<IUser>().ParseAsync(parameter, input, context);
					if (userResult.IsSuccessful) {
						result = (await context.ServiceProvider.GetRequiredService<UserConfigService>().GetConfigAsync(userResult.Value.GetReference())).GetStudentSet();
						byMention = true;
					} else {
						return Unsuccessful(false, "#StudentSetInfoReader_CheckFailed_Direct");
					}
				}

				if (result is null) {
					string message;
					if (byMention) {
						message = "#StudentSetInfoReader_CheckFailed_MentionUser";
					} else {
						message = "#StudentSetInfoReader_CheckFailed_MentionSelf";
					}
					return Unsuccessful(true, message, context.ChannelConfig.CommandPrefix);
				} else {
					return Successful(result);
				}
			}
		}
	}
}
