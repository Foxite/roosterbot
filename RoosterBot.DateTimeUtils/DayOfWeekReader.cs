using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DayOfWeekReader : RoosterTypeParser {
		public override Type Type => typeof(DayOfWeek);

		public override string TypeDisplayName => "#DayOfWeek_DisplayName";

		protected override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			input = input.ToLower();
			ResourceService resources = services.GetService<ResourceService>();
			CultureInfo culture = context.Culture;
			if (input == resources.GetString(culture, "DayOfWeekReader_Today")) {
				return Task.FromResult(TypeReaderResult.FromSuccess(DateTime.Today.DayOfWeek));
			} else if (input == resources.GetString(culture, "DayOfWeekReader_Tomorrow")) {
				return Task.FromResult(TypeReaderResult.FromSuccess(DateTime.Today.AddDays(1).DayOfWeek));
			}

			string[] weekdays = culture.DateTimeFormat.DayNames;

			int? result = null;
			for (int i = 0; i < weekdays.Length; i++) {
				if (weekdays[i].ToLower().StartsWith(input)) {
					if (result == null) {
						result = i;
					} else {
						return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "#DayOfWeekReader_CheckFailed"));
					}
				}
			}

			if (result.HasValue) {
				return Task.FromResult(TypeReaderResult.FromSuccess(((DayOfWeek[]) typeof(DayOfWeek).GetEnumValues())[result.Value]));
			} else {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "#DayOfWeekReader_CheckFailed"));
			}
		}
	}
}
