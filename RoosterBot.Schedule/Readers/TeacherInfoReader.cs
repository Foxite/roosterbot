using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class TeacherInfoReader : IdentifierInfoReaderBase<TeacherInfo> {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			TypeReaderResult baseResult = await base.ReadAsync(context, input, services);
			if (baseResult.IsSuccess) {
				return TypeReaderResult.FromSuccess(baseResult.Values.First());
			} else {
				TeacherNameService tns = services.GetService<TeacherNameService>();
				IEnumerable<TeacherNameService.TeacherMatch> result = null;

				IUser user = null;
				if (context.Guild != null && MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Guild.GetUserAsync(id);
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(context, "IdentifierInfoReader_Self")) {
					user = context.User;
				}

				if (user == null) {
					if (context.Guild != null) {
						result = tns.Lookup(context.Guild.Id, input);
					}
				} else {
					TeacherInfo teacher = tns.GetTeacherByDiscordUser(context.Guild, user);
					if (teacher != null) {
						result = new[] { new TeacherNameService.TeacherMatch(teacher, 1) };
					}
				}

				if (result == null || result.FirstOrDefault() == null) {
					return TypeReaderResult.FromError(CommandError.ParseFailed, services.GetService<ResourceService>().GetString(context, "TeacherInfoReader_CheckFailed"));
				} else {
					TypeReaderResult typeReaderResult = TypeReaderResult.FromSuccess(result.Select(teacher => new TypeReaderValue(teacher.Teacher, teacher.Score)).ToList());
					return typeReaderResult;
				}
			}
		}
	}
}
