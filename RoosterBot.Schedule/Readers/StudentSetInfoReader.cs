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
				IUser user;
				bool byMention = false;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return TypeReaderResult.FromError(CommandError.ParseFailed, "#StudentSetInfoReader_CheckFailed_InaccessibleUser");
					}
					byMention = true;
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(context.Culture, "IdentifierInfoReader_Self")) {
					user = context.User;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, "#StudentSetInfoReader_CheckFailed_Direct");
				}
				StudentSetInfo? result = context.UserConfig.GetStudentSet();
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
