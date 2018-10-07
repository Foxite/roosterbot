using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;

namespace RoosterBot {
	public static class Util {
		public static readonly Random RNG = new Random();

		/// <summary>
		/// Format a string array nicely, with commas and an optional "and" between the last two items.
		/// </summary>
		/// <param name="finalDelimiter">Should include a comma and a trailing space.</param>
		public static string FormatStringArray(this string[] array, string finalDelimiter = ", ") {
			string ret = array[0];
			for (int i = 1; i < array.Length - 1; i++) {
				ret += ", " + array[i];
			}
			if (array.Length > 1) {
				ret += finalDelimiter + array[array.Length - 1];
			}
			return ret;
		}

		/// <summary>
		/// Capitalize the first character in a string.
		/// </summary>
		/// From https://stackoverflow.com/a/4405876/3141917
		public static string FirstCharToUpper(this string input) {
			switch (input) {
			case null:
				throw new ArgumentNullException(nameof(input));
			case "":
				throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
			default:
				return input.First().ToString().ToUpper() + input.Substring(1);
			}
		}

		/// <summary>
		/// Gets all the full names of teachers by their abbreviations. Unlike GetSingleTeacherNameFromAbbr, this accepts a comma seperated list of abbreviations, as they are stored
		///  in the schedule CSVs.
		/// </summary>
		public static string GetTeacherNameFromAbbr(string teacherString) {
			string[] abbrs = teacherString.Split(new[] { ", " }, StringSplitOptions.None);
			string ret = "";
			bool anyTeacherAdded = false;
			for (int i = 0; i < abbrs.Length; i++) {
				string thisTeacher = GetSingleTeacherNameFromAbbr(abbrs[i]);

				// The schedule occasionally contains teachers that don't actually exist (like XGVAC2). Skip those.
				if (thisTeacher != null) {
					// This prevents it from adding a comma if this is the first item, or if we've only had nonexistent teachers so far.
					if (anyTeacherAdded) {
						ret += ", ";
					}
					ret += thisTeacher;
					anyTeacherAdded = true;
				}
			}
			return ret;
		}

		/// <summary>
		/// Gets the full name of a single teacher from their abbreviation.
		/// </summary>
		/// <param name="abbr"></param>
		/// <returns></returns>
		public static string GetSingleTeacherNameFromAbbr(string abbr) {
			switch (abbr) {
			case "ATE":
				return "Arnoud Telkamp";
			case "BHN":
				return "Bram den Hond";
			case "CPE":
				return "Chris-Jan Peterse";
			case "CSP":
				return "Cynthia Spier";
			case "DBU":
				return "David Buzzi";
			case "DWO":
				return "Dick Wories";
			case "HAL":
				return "Hyltsje Altenburg";
			case "HBE":
				return "Hsin Chi Berenst";
			case "JBO":
				return "Jaap van Boggelen";
			case "JWO":
				return "Joram Wolters";
			case "LCA":
				return "Laurence Candel";
			case "LEN":
				return "Laura Endert";
			case "LKR":
				return "Lance Krasniqi";
			case "LMU":
				return "Liselotte Mulder";
			case "MJA":
				return "Martijn Jacobs";
			case "MKU":
				return "Martijn Kunstman";
			case "MME":
				return "Marijn Moerbeek";
			case "MRE":
				return "Miriam Reutelingsperger";
			case "MVE":
				return "Maart Veldman";
			case "RBA":
				return "René Balkenende";
			case "RBR":
				return "Rubin de Bruin";
			case "SSC":
				return "Sander Scholl";
			case "SLO":
				return "Suus Looijen";
			case "SRI":
				return "Suzanne Ringeling";
			case "VV-GAGD":
				return "een vervangende docent";
			case "WSC":
				return "Willemijn Schmitz";
			case "YWI":
				return "Yelena de Wit";
			default:
				return null;
			}
		}

		/// <summary>
		/// Gets the abbreviation of one or more teachers with the given name, or returns the input if it is an abbreviation.
		/// It supports the first names. If multiple teachers have the same first name, it will return a comma seperated list (as a string) of all teachers with that name.
		/// It also supports some alternative spellings of first names, such as "chris-jan" or "chrisjan", "rené" or "rene".
		/// </summary>
		public static string GetTeacherAbbrFromName(string name) {
			if (name.Length < 3)
				return null;

			if (name.ToLower() == "martijn kunstman")
				return "MKU";
			if (name.ToLower() == "martijn jacobs")
				return "MJA";

			switch (name.Split(' ')[0].ToLower()) {
			case "ate":
			case "arnoud":
				return "ATE";
			case "bhn":
			case "bram":
				return "BHN";
			case "cpe":
			case "chris-jan":
			case "chrisjan":
				return "CPE";
			case "csp":
			case "cynthia":
				return "CSP";
			case "dbu":
			case "david":
				return "DBU";
			case "dwo":
			case "dick":
				return "DWO";
			case "hal":
			case "hyltsje":
				return "HAL";
			case "hbe":
			case "hsin":
			case "chi":
				return "HBE";
			case "jbo":
			case "jaap":
				return "JBO";
			case "jwo":
			case "joram":
				return "JWO";
			case "len":
			case "laura":
				return "LEN";
			case "lca":
			case "laurence":
			case "laurens":
				return "LCA";
			case "lkr":
			case "lance":
				return "LKR";
			case "lmu":
			case "liselotte":
				return "LMU";
			case "mja":
				return "MJA";
			case "mku":
				return "MKU";
			case "martijn":
				return "MJA, MKU";
			case "mme":
			case "marijn":
				return "MME";
			case "mre":
			case "miriam":
				return "MRE";
			case "mve":
			case "maart":
				return "MVE";
			case "rba":
			case "rené":
			case "rene":
				return "RBA";
			case "rbr":
			case "rubin":
				return "RBR";
			case "slo":
			case "suus":
				return "SLO";
			case "sri":
			case "suzanne":
				return "SRI";
			case "ssc":
			case "sander":
				return "Sander Scholl";
			case "ywi":
			case "yelena":
				return "YWI";
			case "wsc":
			case "willemijn":
				return "Willemijn Schmitz";
			}
			return null;
		}

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

			case "3d":
			case "2d":
			case "bpv":
			case "vb bpv":
			case "2d/3d":
			case "slb":
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
				return abbr.FirstCharToUpper();

			default:
				return $"\"{abbr}\" (ik weet de volledige naam niet)";
			}
		}
		
		/// <summary>Adds a reaction to an IUserMessage. Only supports Emoji, not server-specific emotes.</summary>
		/// <returns>Success. It can fail if the bot does not have permission to add reactions.</returns>
		public static async Task<bool> AddReaction(IUserMessage message, string unicode) {
			try {
				await message.AddReactionAsync(new Emoji(unicode));
				return true;
			} catch (HttpException) {
				return false;
			} // Permission denied
		}
		
		/// <summary>
		/// Given either the name of a weekdag in Dutch, or the first two letters, this returns a DayOfWeek corresponding to the input.
		/// </summary>
		/// <exception cref="System.ArgumentException">When the input is not the full name or the first two letters of a weekday in Dutch.</exception>
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
			default:
				throw new ArgumentException(dayofweek + " is not a weekday.");
			}
		}

		/// <summary>
		/// Given a DayOfWeek, this returns the Dutch name of that day.
		/// </summary>
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

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}
}
