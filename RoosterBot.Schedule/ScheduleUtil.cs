using System;
using System.Globalization;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		/// <summary>
		/// Given a DayOfWeek, this returns the Dutch name of that day.
		/// </summary>
		public static string GetStringFromDayOfWeek(CultureInfo culture, DayOfWeek day) {
			return culture.DateTimeFormat.DayNames[(int) day];
		}

		}

		/// <summary>
		/// Returns the first DateTime after today of which the DayOfWeek is equal to <paramref name="day"/>
		/// </summary>
		/// <param name="includeToday">If today is suitable: If true, then this will return today. If false, it will return 7 days from now.</param>
		public static DateTime NextDayOfWeek(DayOfWeek day, bool includeToday) {
			// https://stackoverflow.com/a/6346190/3141917
			DateTime targetDate;
			if (includeToday) {
				// Get the next {day} including today
				targetDate = DateTime.Today.AddDays(((int)day - (int)DateTime.Today.DayOfWeek + 7) % 7);
			} else {
				// Get the next {day} after today
				targetDate = DateTime.Today.AddDays(1 + ((int)day - (int)DateTime.Today.AddDays(1).DayOfWeek + 7) % 7);
			}

			return targetDate;
		}

		/// <summary>
		/// For a given DateTime, this returns the Dutch relative reference for that date.
		/// For example, today, tomorrow, on Tueday, or if it's 7 or more days away, it will return <code>date.ToString("dd-MM")</code>.
		/// </summary>
		/// <param name="date"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		// TODO localize relative date reference
		public static string GetRelativeDateReference(CultureInfo culture, DateTime date) {
			if (date == DateTime.Today) {
				return "vandaag";
			} else if (date == DateTime.Today.AddDays(1)) {
				return "morgen";
			} else if ((date - DateTime.Today).TotalDays < 7) {
				return "op " + GetStringFromDayOfWeek(culture, date.DayOfWeek);
			} else {
				return "op " + date.ToString("dd-MM");
			}
		}

		public static bool IsWeekend(DateTime date) {
			return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
		}

		public static bool IsWithinSameWeekend(DateTime left, DateTime right) {
			return IsWeekend(left) && IsWeekend(right) && Math.Abs((left.Date - right.Date).TotalDays) <= 2;
		}
	}
}
