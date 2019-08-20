using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Readers {
	public class TeacherInfoReader : TypeReader {
		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			TeacherNameService tns = services.GetService<TeacherNameService>();
			TeacherInfo[] result = null;
			
			IUser user = null;
			if (context.Guild != null && MentionUtils.TryParseUser(input, out ulong id)) {
				user = await context.Guild.GetUserAsync(id);
			} else if (input == "ik") {
				user = context.User;
			}

			TeacherInfo teacher = null;

			if (user != null) {
				teacher = tns.GetTeacherByDiscordUser(user);
				if (teacher != null) {
					result = new TeacherInfo[] { teacher };
				}
			}
			
			if (teacher == null) {
				result = tns.Lookup(input);
			}

			if (result == null || result.Length == 0) {
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				return TypeReaderResult.FromSuccess(result);
			}
		}
	}
}
