using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.DateTimeUtils {
	public class DayOfWeekReader : RoosterTypeParser<DayOfWeek> {
		public override string TypeDisplayName => "#DayOfWeek_DisplayName";

		protected override ValueTask<TypeParserResult<DayOfWeek>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			input = input.ToLower();
			ResourceService resources = context.ServiceProvider.GetService<ResourceService>();
			CultureInfo culture = context.Culture;
			if (input == resources.GetString(culture, "DayOfWeekReader_Today")) {
				return new ValueTask<TypeParserResult<DayOfWeek>>(TypeParserResult<DayOfWeek>.Successful(DateTime.Today.DayOfWeek));
			} else if (input == resources.GetString(culture, "DayOfWeekReader_Tomorrow")) {
				return new ValueTask<TypeParserResult<DayOfWeek>>(TypeParserResult<DayOfWeek>.Successful(DateTime.Today.AddDays(1).DayOfWeek));
			}

			string[] weekdays = culture.DateTimeFormat.DayNames;

			int? result = null;
			for (int i = 0; i < weekdays.Length; i++) {
				if (weekdays[i].ToLower().StartsWith(input)) {
					if (result == null) {
						result = i;
					} else {
						return new ValueTask<TypeParserResult<DayOfWeek>>(TypeParserResult<DayOfWeek>.Unsuccessful("#DayOfWeekReader_CheckFailed"));
					}
				}
			}

			if (result.HasValue) {
				return new ValueTask<TypeParserResult<DayOfWeek>>(TypeParserResult<DayOfWeek>.Successful(((DayOfWeek[]) typeof(DayOfWeek).GetEnumValues())[result.Value]));
			} else {
				return new ValueTask<TypeParserResult<DayOfWeek>>(TypeParserResult<DayOfWeek>.Unsuccessful("#DayOfWeekReader_CheckFailed"));
			}
		}
	}
}
