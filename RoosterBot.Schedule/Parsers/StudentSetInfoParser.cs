using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class StudentSetInfoParser : IdentifierInfoParserBase<StudentSetInfo> {
		public override string TypeDisplayName => "#StudentSetInfo_TypeDisplayName";
		
		public StudentSetInfoParser(Component component) : base (component) { }

		protected async override ValueTask<RoosterTypeParserResult<StudentSetInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			RoosterTypeParserResult<StudentSetInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				bool byMention;
				StudentSetInfo? result;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					IUser user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return Unsuccessful(true, "#StudentSetInfoReader_CheckFailed_InaccessibleUser");
					}
					result = (await context.ServiceProvider.GetService<UserConfigService>().GetConfigAsync(user)).GetStudentSet();
					byMention = true;
				} else if (input.ToLower() == context.ServiceProvider.GetService<ResourceService>().GetString(context.Culture, "IdentifierInfoReader_Self")) {
					result = context.UserConfig.GetStudentSet();
					byMention = false;
				} else {
					return Unsuccessful(false, "#StudentSetInfoReader_CheckFailed_Direct");
				}
				if (result is null) {
					string message;
					if (byMention) {
						message = "#StudentSetInfoReader_CheckFailed_MentionUser";
					} else {
						message = "#StudentSetInfoReader_CheckFailed_MentionSelf";
					}
					return Unsuccessful(true, message, context.GuildConfig.CommandPrefix);
				} else {
					return Successful(result);
				}
			}
		}
	}
}
