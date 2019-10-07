using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DayOfWeekReader : RoosterTypeReaderBase {
		protected override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			input = input.ToLower();
			ResourceService resources = services.GetService<ResourceService>();
			if (input == resources.GetString(context, "DayOfWeekReader_Today")) {
				Task.FromResult(TypeReaderResult.FromSuccess(DateTime.Today.DayOfWeek));
			} else if (input == resources.GetString(context, "DayOfWeekReader_Tomorrow")) {
				return Task.FromResult(TypeReaderResult.FromSuccess(DateTime.Today.AddDays(1).DayOfWeek));
			}

			string[] weekdays = services.GetService<GuildCultureService>().GetCultureForGuild(context.Guild).DateTimeFormat.DayNames;

			int? result = null;
			for (int i = 0; i < weekdays.Length; i++) {
				if (weekdays[i].ToLower().StartsWith(input)) {
					if (result == null) {
						result = i;
					} else {
						return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context, "DayOfWeekReader_CheckFailed")));
					}
				}
			}

			if (result.HasValue) {
				return Task.FromResult(TypeReaderResult.FromSuccess(((DayOfWeek[]) typeof(DayOfWeek).GetEnumValues())[result.Value]));
			} else {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context, "DayOfWeekReader_CheckFailed")));
			}
		}
	}
}
