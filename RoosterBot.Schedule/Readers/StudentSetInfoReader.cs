using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class StudentSetInfoReader : IdentifierInfoReaderBase<StudentSetInfo> {
		public override string TypeDisplayName => "#StudentSetInfo_TypeDisplayName";

		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			TypeReaderResult baseResult = await base.ReadAsync(context, input, services);
			if (baseResult.IsSuccess) {
				return baseResult;
			} else {
				ResourceService resources = services.GetService<ResourceService>();
				bool byMention;
				StudentSetInfo? result;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					IUser user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_InaccessibleUser"));
					}
					result = (await services.GetService<UserConfigService>().GetConfigAsync(user)).GetStudentSet();
					byMention = true;
				} else if (input.ToLower() == resources.GetString(context.Culture, "IdentifierInfoReader_Self")) {
					result = context.UserConfig.GetStudentSet();
					byMention = false;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_Direct"));
				}
				if (result is null) {
					string message;
					if (byMention) {
						message = string.Format(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_MentionUser"), context.GuildConfig.CommandPrefix);
					} else {
						message = string.Format(resources.GetString(context.Culture, "StudentSetInfoReader_CheckFailed_MentionSelf"), context.GuildConfig.CommandPrefix);
					}
					return TypeReaderResult.FromError(CommandError.Unsuccessful, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
