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
			if (MentionUtils.TryParseUser(input, out ulong id)) {
				user = await context.Client.GetUserAsync(id);
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
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				return TypeReaderResult.FromSuccess(result);
			}
		}
	}
}
