﻿using System;
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

				IGuild guild = context.Guild ?? await context.GetDMGuildAsync();
				if (user == null) {
					if (guild != null) {
						result = tns.Lookup(guild.Id, input);
					}
				} else {
					TeacherInfo teacher = tns.GetTeacherByDiscordUser(guild, user);
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
