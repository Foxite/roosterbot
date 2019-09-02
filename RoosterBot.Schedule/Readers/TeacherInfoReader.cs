using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class TeacherInfoReader : IdentifierInfoReaderBase<TeacherInfo> {
		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			TypeReaderResult baseResult = await base.ReadAsync(context, input, services);
			if (baseResult.IsSuccess) {
				return TypeReaderResult.FromSuccess(new[] { baseResult.Values.First() });
			} else {
				TeacherNameService tns = services.GetService<TeacherNameService>();
				TeacherInfo[] result = null;

				IUser user = null;
				if (context.Guild != null && MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Guild.GetUserAsync(id);
				} else if (input == "ik") {
					user = context.User;
				}

				if (user == null) {
					result = tns.Lookup(context.Guild.Id, input);
				} else {
					TeacherInfo teacher = tns.GetTeacherByDiscordUser(context.Guild, user);
					if (teacher != null) {
						result = new TeacherInfo[] { teacher };
					}
				}

				if (result == null || result.Length == 0) {
					return TypeReaderResult.FromError(CommandError.ParseFailed, Resources.TeacherInfoReader_CheckFailed);
				} else {
					return TypeReaderResult.FromSuccess(result);
				}
			}
		}
	}
}
