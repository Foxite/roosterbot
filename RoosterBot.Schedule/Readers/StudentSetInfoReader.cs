using System;
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
				IUser user;
				bool byMention = false;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Client.GetUserAsync(id); // Should have a value
					byMention = true;
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(context, "IdentifierInfoReader_Self")) {
					user = context.User;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context, "StudentSetInfoReader_CheckFailed_Direct"));
				}
				StudentSetInfo result = await services.GetService<IUserClassesService>().GetClassForDiscordUserAsync(context, user);
				if (result is null) {
					string message;
					if (byMention) {
						message = resources.GetString(context, "StudentSetInfoReader_CheckFailed_MentionUser");
					} else {
						message = resources.GetString(context, "StudentSetInfoReader_CheckFailed_MentionSelf");
					}
					return TypeReaderResult.FromError(CommandError.Unsuccessful, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
