using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DayOfWeekReader : RoosterTypeReader {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			input = input.ToLower();
			ResourceService resources = services.GetService<ResourceService>();
			CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild))!.Culture;
			if (input == resources.GetString(culture, "DayOfWeekReader_Today")) {
				return TypeReaderResult.FromSuccess(DateTime.Today.DayOfWeek);
			} else if (input == resources.GetString(culture, "DayOfWeekReader_Tomorrow")) {
				return TypeReaderResult.FromSuccess(DateTime.Today.AddDays(1).DayOfWeek);
			}

			string[] weekdays = culture.DateTimeFormat.DayNames;

			int? result = null;
			for (int i = 0; i < weekdays.Length; i++) {
				if (weekdays[i].ToLower().StartsWith(input)) {
					if (result == null) {
						result = i;
					} else {
						return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(culture, "DayOfWeekReader_CheckFailed"));
					}
				}
			}

			if (result.HasValue) {
				return TypeReaderResult.FromSuccess(((DayOfWeek[]) typeof(DayOfWeek).GetEnumValues())[result.Value]);
			} else {
				return TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(culture, "DayOfWeekReader_CheckFailed"));
			}
		}
	}
}
