using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Readers {
	public class StudentSetInfoReader : TypeReader {
		private static readonly Regex s_StudentSetRegex = new Regex("^[1-4][Gg][ADad][12]$");
		internal static readonly Regex s_LookupRegex = new Regex("^\\<\\@\\!?[0-9]{1,19}\\>$");

		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			if (s_StudentSetRegex.IsMatch(input)) {
				return TypeReaderResult.FromSuccess(new StudentSetInfo() {
					ClassName = input.ToUpper()
				});
			} else {
				IUser user;
				bool byMention = false;
				/*if (s_LookupRegex.IsMatch(input)) {
					user = await context.Client.GetUserAsync(Util.ExtractIDFromMentionString(input).Value); // Should have a value
				} else*/ if (input.ToLower() == "ik") {
					user = context.User;
					byMention = true;
				} else {
					return TypeReaderResult.FromError(CommandError.ParseFailed, "Dat is geen klas.");
				}
				StudentSetInfo result = await services.GetService<UserClassesService>().GetClassForDiscordUser(user);
				if (result is null) {
					string message;
					if (byMention) {
						message = "Ik weet niet in welke klas die persoon zit. Hij/zij moet `!ik <zijn/haar klas>` gebruiken om dit in te stellen.";
					} else {
						message = "Ik weet niet in welke klas jij zit. Gebruik `!ik <jouw klas>` om dit in te stellen.";
					}
					return TypeReaderResult.FromError(CommandError.ParseFailed, message);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
