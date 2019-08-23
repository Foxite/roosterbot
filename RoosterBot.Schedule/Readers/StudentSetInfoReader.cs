using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class StudentSetInfoReader : TypeReader {
		private static readonly Regex s_StudentSetRegex = new Regex("^[1-4][Gg][ADad][12]$");

		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			if (s_StudentSetRegex.IsMatch(input)) {
				return TypeReaderResult.FromSuccess(new StudentSetInfo() {
					ClassName = input.ToUpper()
				});
			} else {
				IUser user;
				bool byMention = false;
				if (MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Client.GetUserAsync(id); // Should have a value
				} else if (input.ToLower() == "ik") {
					user = context.User;
					byMention = true;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, Resources.StudentSetInfoReader_CheckFailed_Direct);
				}
				StudentSetInfo result = await services.GetService<UserClassesService>().GetClassForDiscordUser(user);
				if (result is null) {
					string message;
					if (byMention) {
						message = Resources.StudentSetInfoReader_CheckFailed_MentionUser;
					} else {
						message = Resources.StudentSetInfoReader_CheckFailed_MentionSelf;
					}
					return TypeReaderResult.FromError(CommandError.ParseFailed, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
