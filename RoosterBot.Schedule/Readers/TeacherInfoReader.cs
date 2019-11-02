using System;
using System.Collections.Generic;
using System.Globalization;
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
				CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
				TeacherNameService tns = services.GetService<TeacherNameService>();
				IEnumerable<TeacherMatch>? result = null;

				IUser? user = null;
				if (context.Guild != null && MentionUtils.TryParseUser(input, out ulong id)) {
					user = await context.Guild.GetUserAsync(id);
				} else if (input.ToLower() == services.GetService<ResourceService>().GetString(culture, "IdentifierInfoReader_Self")) {
					user = context.User;
				}

				if (user == null) {
					if (context.Guild != null) {
						result = tns.Lookup(context.Guild.Id, input);
					}
				} else {
					IGuild? lookupGuild = context.Guild ?? context.UserGuild;
					if (lookupGuild != null) {
						TeacherInfo? teacher = tns.GetTeacherByDiscordUser(lookupGuild, user);
						if (teacher != null) {
							result = Util.Pack(new TeacherMatch(teacher, 1));
						}
					}
				}

				if (result == null || result.FirstOrDefault() == null) {
					return TypeReaderResult.FromError(CommandError.ParseFailed, services.GetService<ResourceService>().GetString(culture, "TeacherInfoReader_CheckFailed"));
				} else {
					TypeReaderResult typeReaderResult = TypeReaderResult.FromSuccess(result.Select(teacher => new TypeReaderValue(teacher.Teacher, teacher.Score)).ToList());
					return typeReaderResult;
				}
			}
		}
	}
}
