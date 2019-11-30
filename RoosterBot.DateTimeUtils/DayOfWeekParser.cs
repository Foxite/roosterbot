using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.DateTimeUtils {
	public class DayOfWeekParser : RoosterTypeParser<DayOfWeek> {
		public override string TypeDisplayName => "#DayOfWeek_DisplayName";

		public DayOfWeekParser(Component component) : base(component) { }

		protected override ValueTask<RoosterTypeParserResult<DayOfWeek>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			input = input.ToLower();
			ResourceService resources = context.ServiceProvider.GetService<ResourceService>();
			CultureInfo culture = context.Culture;
			if (input == resources.GetString(culture, "DayOfWeekReader_Today")) {
				return ValueTaskUtil.FromResult(Successful(DateTime.Today.DayOfWeek));
			} else if (input == resources.GetString(culture, "DayOfWeekReader_Tomorrow")) {
				return ValueTaskUtil.FromResult(Successful(DateTime.Today.AddDays(1).DayOfWeek));
			}

			string[] weekdays = culture.DateTimeFormat.DayNames;

			int? result = null;
			for (int i = 0; i < weekdays.Length; i++) {
				if (weekdays[i].ToLower().StartsWith(input)) {
					if (result == null) {
						result = i;
					} else {
						return ValueTaskUtil.FromResult(Unsuccessful(false, "#DayOfWeekReader_CheckFailed"));
					}
				}
			}

			if (result.HasValue) {
				return ValueTaskUtil.FromResult(Successful(((DayOfWeek[]) typeof(DayOfWeek).GetEnumValues())[result.Value]));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, "#DayOfWeekReader_CheckFailed"));
			}
		}
	}
}
