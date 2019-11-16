using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class StudentSetInfoReader : IdentifierInfoReaderBase<StudentSetInfo> {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			TypeReaderResult baseResult = await base.ReadAsync(context, input, services);
			if (baseResult.IsSuccess) {
				return baseResult;
			} else {
				ResourceService resources = services.GetService<ResourceService>();
				GuildConfig guildConfig = context.GuildConfig;
				UserConfig userConfig = await services.GetRequiredService<UserConfigService>().GetConfigAsync(context.User);
				IUser user;
				bool byMention = false;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return TypeReaderResult.FromError(CommandError.ParseFailed, "#StudentSetInfoReader_CheckFailed_InaccessibleUser");
					}
					byMention = true;
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(guildConfig.Culture, "IdentifierInfoReader_Self")) {
					user = context.User;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, "#StudentSetInfoReader_CheckFailed_Direct");
				}
				StudentSetInfo? result = userConfig.GetStudentSet();
				if (result is null) {
					string message;
					if (byMention) {
						message = string.Format(resources.GetString(guildConfig.Culture, "StudentSetInfoReader_CheckFailed_MentionUser"), guildConfig.CommandPrefix);
					} else {
						message = string.Format(resources.GetString(guildConfig.Culture, "StudentSetInfoReader_CheckFailed_MentionSelf"), guildConfig.CommandPrefix);
					}
					return TypeReaderResult.FromError(CommandError.Unsuccessful, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
