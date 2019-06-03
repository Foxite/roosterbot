using RoosterBot;
using System;

namespace ScheduleComponent {
	public static class ScheduleUtil {
		/// <summary>
		/// Get the full name for an activity from its abbreviation.
		/// </summary>
		public static string GetActivityFromAbbr(string abbr) {
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
	}
}
