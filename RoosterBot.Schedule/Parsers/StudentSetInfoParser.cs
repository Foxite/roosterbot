using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class StudentSetInfoParser : IdentifierInfoParserBase<StudentSetInfo> {
		public override string TypeDisplayName => "#StudentSetInfo_TypeDisplayName";

		protected async override ValueTask<TypeParserResult<StudentSetInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			TypeParserResult<StudentSetInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				ResourceService resources = context.ServiceProvider.GetService<ResourceService>();
				bool byMention;
				StudentSetInfo? result;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					IUser user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return TypeParserResult<StudentSetInfo>.Unsuccessful(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_InaccessibleUser"));
					}
					result = (await context.ServiceProvider.GetService<UserConfigService>().GetConfigAsync(user)).GetStudentSet();
					byMention = true;
				} else if (input.ToLower() == resources.GetString(context.Culture, "IdentifierInfoReader_Self")) {
					result = context.UserConfig.GetStudentSet();
					byMention = false;
				} else {
					return TypeParserResult<StudentSetInfo>.Unsuccessful(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_Direct"));
				}
				if (result is null) {
					string message;
					if (byMention) {
						message = string.Format(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_MentionUser"), context.GuildConfig.CommandPrefix);
					} else {
						message = string.Format(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_MentionSelf"), context.GuildConfig.CommandPrefix);
					}
					return TypeParserResult<StudentSetInfo>.Unsuccessful(message);
				} else {
					return TypeParserResult<StudentSetInfo>.Successful(result);
				}
			}
		}
	}
}
