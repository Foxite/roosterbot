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
				GuildConfig config = await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild ?? context.UserGuild);
				IUser user;
				bool byMention = false;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Client.GetUserAsync(id);
					if (user == null) {
						return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(config.Culture, "StudentSetInfoReader_CheckFailed_InaccessibleUser"));
					}
					byMention = true;
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(config.Culture, "IdentifierInfoReader_Self")) {
					user = context.User;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(config.Culture, "StudentSetInfoReader_CheckFailed_Direct"));
				}
				StudentSetInfo? result = await services.GetService<IUserClassesService>().GetClassForDiscordUserAsync(context, user);
				if (result is null) {
					string message;
					if (byMention) {
						message = string.Format(resources.GetString(config.Culture, "StudentSetInfoReader_CheckFailed_MentionUser"), config.CommandPrefix);
					} else {
						message = string.Format(resources.GetString(config.Culture, "StudentSetInfoReader_CheckFailed_MentionSelf"), config.CommandPrefix);
					}
					return TypeReaderResult.FromError(CommandError.Unsuccessful, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
