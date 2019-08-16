﻿using System;
using RoosterBot;

namespace ScheduleComponent {
	public static class ScheduleUtil {
		/// <summary>
		/// Get the full name for an activity from its abbreviation.
		/// </summary>
		public static string GetActivityFromAbbr(string abbr) {
			// TODO this needs to be per-guild
			switch (abbr) {
				case "ned":
					return "Nederlands";
				case "eng":
					return "Engels";
				case "program":
					return "Programmeren";
				case "gamedes":
					return "Gamedesign";
				case "ond":
					return "Onderneming";
				case "k0072":
					return "Keuzedeel (k0072)";
				case "k0821":
					return "Keuzedeel (k0821)";
				case "k0901":
					return "Keuzedeel (k0901)";
				case "burger":
					return "Burgerschap";
				case "rek":
					return "Rekenen";
				case "vormg":
					return "Vormgeving";
				case "engine":
					return "Engineering";
				case "stdag doc":
					return "Studiedag :tada:";
				case "to":
					return "Teamoverleg";
				case "skc":
					return "Studiekeuzecheck";
				case "soll":
					return "Solliciteren";
				case "mastercl":
					return "Masterclass";

				case "3d":
				case "2d":
				case "bpv":
				case "vb bpv":
				case "vb pvb":
				case "2d/3d":
				case "slb":
				case "avo":
					return abbr.ToUpper();

				case "pauze":
				case "gameaudio":
				case "keuzedeel":
				case "gametech":
				case "project":
				case "rapid":
				case "gameplay":
				case "taken":
				case "stage":
				case "examen":
				case "animatie":
				case "werkveld":
				case "afstudeer":
				case "rozosho":
				case "rozosho-i":
				case "twinstick":
					return abbr.FirstCharToUpper();

				case "Sinterklaas":
					return abbr;

				default:
					return $"\"{abbr}\"";
			}
		}

		/// <summary>
		/// Given either the name of a weekdag in Dutch, or the first two letters, this returns a DayOfWeek corresponding to the input.
		/// </summary>
		/// <exception cref="ArgumentException">When the input is not the full name or the first two letters of a weekday in Dutch.</exception>
		public static DayOfWeek GetDayOfWeekFromString(string dayofweek) {
			switch (dayofweek.ToLower()) {
				case "ma":
				case "maandag":
					return DayOfWeek.Monday;
				case "di":
				case "dinsdag":
					return DayOfWeek.Tuesday;
				case "wo":
				case "woensdag":
					return DayOfWeek.Wednesday;
				case "do":
				case "donderdag":
					return DayOfWeek.Thursday;
				case "vr":
				case "vrijdag":
					return DayOfWeek.Friday;
				case "za":
				case "zaterdag":
					return DayOfWeek.Saturday;
				case "zo":
				case "zondag":
					return DayOfWeek.Sunday;
				case "vandaag":
					return DateTime.Today.DayOfWeek;
				case "morgen":
					return DateTime.Today.AddDays(1).DayOfWeek;
				default:
					throw new ArgumentException(dayofweek + " is not a weekday.");
			}
		}

		/// <summary>
		/// Given a DayOfWeek, this returns the Dutch name of that day.
		/// </summary>
		/// <exception cref="ArgumentException">When the input is not the full name or the first two letters of a weekday in Dutch.</exception>
		public static string GetStringFromDayOfWeek(DayOfWeek day) {
			switch (day) {
				case DayOfWeek.Monday:
					return "maandag";
				case DayOfWeek.Tuesday:
					return "dinsdag";
				case DayOfWeek.Wednesday:
					return "woensdag";
				case DayOfWeek.Thursday:
					return "donderdag";
				case DayOfWeek.Friday:
					return "vrijdag";
				case DayOfWeek.Saturday:
					return "zaterdag";
				case DayOfWeek.Sunday:
					return "zondag";
				default:
					throw new ArgumentException();
			}
		}

		/// <summary>
		/// Tests if the given DateTime is within the summer break (between july 20 and september 1 of any year, inclusive)
		/// </summary>
		/// <param name="dateTime">If null, will use today.</param>
		/// <returns></returns>
		public static bool IsSummerBreak(DateTime? dateTime = null) {
			DateTime dt;
			if (dateTime.HasValue) {
				dt = dateTime.Value;
			} else {
				dt = DateTime.Today;
			}
			DateTime startBreak = new DateTime(2019, 07, 20);
			DateTime endBreak = new DateTime(2019, 09, 01);
			return dt >= startBreak && dt <= endBreak;
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
		/// For example, today, tomorrow, on Tueday, or if it's 7 or more days away, it will return <code>date.ToStrin("dd-MM")</code>.
		/// </summary>
		/// <param name="date"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		public static string GetRelativeDateReference(DateTime date) {
			if (date == DateTime.Today) {
				return "vandaag";
			} else if (date == DateTime.Today.AddDays(1)) {
				return "morgen";
			} else if ((date - DateTime.Today).TotalDays < 7) {
				return "op " + GetStringFromDayOfWeek(date.DayOfWeek);
			} else {
				return "op " + date.ToString("dd-MM");
			}
		}
	}
}
