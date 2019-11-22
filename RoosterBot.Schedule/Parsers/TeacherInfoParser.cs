using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public class TeacherInfoParser : IdentifierInfoParserBase<TeacherInfo> {
		public override string TypeDisplayName => "#TeacherInfo_TypeDisplayName";

		protected async override ValueTask<TypeParserResult<TeacherInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			TypeParserResult<TeacherInfo> baseResult = await base.ParseAsync(parameter, input, context);
			if (baseResult.IsSuccessful) {
				return baseResult;
			} else {
				CultureInfo culture = context.Culture;
				TeacherNameService tns = context.ServiceProvider.GetService<TeacherNameService>();
				ResourceService resourceService = context.ServiceProvider.GetService<ResourceService>();
				TeacherInfo? result = null;

				IUser? user = null;
				if (context.Guild != null && MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Guild.GetUserAsync(id);
				} else {
					if (input.ToLower() == resourceService.GetString(culture, "IdentifierInfoReader_Self")) {
						user = context.User;
					}
				}

				if (user == null) {
					IReadOnlyCollection<TeacherMatch> lookupResults = tns.Lookup(context.GuildConfig.GuildId, input);
					TeacherMatch bestMatch = lookupResults.FirstOrDefault();
					if (bestMatch != null) {
						foreach (TeacherMatch match in lookupResults.Skip(1)) {
							if (match.Score > bestMatch.Score) {
								bestMatch = match;
							}
						}
						result = bestMatch.Teacher;
					}
				} else {
					if (context.Guild != null) {
						TeacherInfo? teacher = tns.GetTeacherByDiscordUser(context.Guild, user);
						if (teacher != null) {
							result = teacher;
						}
					}
				}

				if (result == null) {
					return TypeParserResult<TeacherInfo>.Unsuccessful(resourceService.GetString(culture, "TeacherInfoReader_CheckFailed"));
				} else {
					return TypeParserResult<TeacherInfo>.Successful(result);
				}
			}
		}
	}
}
