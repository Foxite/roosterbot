﻿using System;
using System.Globalization;

namespace RoosterBot.DateTimeUtils {
	public static class DateTimeUtil {
		public static string GetName(this DayOfWeek day, CultureInfo culture) {
			return culture.DateTimeFormat.DayNames[(int) day];
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
				targetDate = DateTime.Today.AddDays(((int) day - (int) DateTime.Today.DayOfWeek + 7) % 7);
			} else {
				// Get the next {day} after today
				targetDate = DateTime.Today.AddDays(1 + ((int) day - (int) DateTime.Today.AddDays(1).DayOfWeek + 7) % 7);
			}

			return targetDate;
		}

		public static bool IsWeekend(DateTime date) {
			return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
		}

		public static string GetRelativeDateReference(DateTime date, CultureInfo culture) {
			string GetString(string key, params string[] objects) {
				return string.Format(DateTimeUtilsComponent.ResourceService.GetString(culture, key), objects);
			}

			if (date == DateTime.Today) {
				return GetString("RelativeDateReference_Today");
			} else if (date == DateTime.Today.AddDays(1)) {
				return GetString("RelativeDateReference_Tomorrow");
			} else if (Math.Abs((date - DateTime.Today).TotalDays) < 7) {
				return GetString("RelativeDateReference_DayName", date.DayOfWeek.GetName(culture));
			} else {
				return GetString("RelativeDateReference_Date", date.ToString("D", culture));
			}
		}
	}
}
