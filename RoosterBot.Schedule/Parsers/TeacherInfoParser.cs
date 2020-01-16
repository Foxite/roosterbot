using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class TeacherInfoParser : IdentifierInfoParserBase<TeacherInfo> {
		public override string TypeDisplayName => "#TeacherInfoReader_TypeDisplayName";

		public async override ValueTask<RoosterTypeParserResult<TeacherInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			RoosterTypeParserResult<TeacherInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				TeacherNameService tns = context.ServiceProvider.GetService<TeacherNameService>();
				TeacherInfo? result = null;

				IUser? user = null;
				TypeParserResult<IUser> userResult = await context.ServiceProvider.GetService<RoosterCommandService>().GetPlatformSpecificParser<IUser>().ParseAsync(parameter, input, context);
				if (userResult.IsSuccessful) {
					user = userResult.Value;
				}

				if (user == null) {
					if (input.Length >= 3) {
						IReadOnlyCollection<TeacherMatch> lookupResults = tns.Lookup(context.ChannelConfig.ChannelReference, input);
						if (lookupResults.Count == 1) {
							result = lookupResults.First().Teacher;
						} else if (lookupResults.Count > 1) {
							return Unsuccessful(true, context, "#TeacherInfoReader_MultipleMatches", string.Join(", ", lookupResults.Select(match => match.Teacher.DisplayText)));
						}
					}
				} else if (context.IsPrivate) {
					result = tns.GetTeacherByDiscordUser(context.Channel.GetReference(), user);
				}

				if (result == null) {
					return Unsuccessful(false, context, "#TeacherInfoReader_CheckFailed");
				} else {
					return Successful(result);
				}
			}
		}
	}
}
