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
			if (array.Length > 0) {
				string ret = array[0];
				for (int i = 1; i < array.Length - 1; i++) {
					ret += ", " + array[i];
				}
				if (array.Length > 1) {
					ret += finalDelimiter + array[array.Length - 1];
				}
				return ret;
			} else {
				return null;
			}
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
			case "to":
				return "Teamoverleg";

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
			case "animatie":
				return abbr.FirstCharToUpper();

			case "Sinterklaas":
				return abbr;

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
			} catch (HttpException) { // Permission denied
				return false;
			}
		}

		/// <summary>Removes our own reaction to an IUserMessage. If we have not added this reaction, nothing will happen. Only supports Emoji, not server-specific emotes.</summary>
		/// <param name="botUser">The bot's user.</param>
		/// <returns>Success. It can fail if the bot does not have permission to add reactions.</returns>
		public static async Task<bool> RemoveReaction(IUserMessage message, string unicode, IUser botUser) {
			try {
				await message.RemoveReactionAsync(new Emoji(unicode), botUser);
				return true;
			} catch (HttpException) { // Permission denied
				return false;
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
		/// Generate a table for use in a Discord message. Output will be formatted into a code block.
		/// Warning: This will glitch out when the table array is jagged.
		/// </summary>
		/// <param name="table">An array[row][column]. While it can be jagged, if it is jagged, this will not work properly.</param>
		/// <param name="includeHeaderSeperation">Include a line of '-'s after the first row.</param>
		/// <returns>A string that you can send directly into a chat message.</returns>
		public static string FormatTextTable(string[][] table, bool includeHeaderSeperation) {
			int[] columnWidths = new int[table[0].Length];
			
			for (int column = 0; column < table[0].Length; column++) {
				columnWidths[column] = 1;
				for (int row = 0; row < table.Length; row++) {
					columnWidths[column] = Math.Max(columnWidths[column], table[row][column].Length);
				}
			}

			string ret = "```";
			for (int row = 0; row < table.Length; row++) {
				for (int column = 0; column < table[0].Length; column++) {
					if (column != 0) {
						ret += " | ";
					}
					ret += table[row][column].PadRight(columnWidths[column]);
				}

				if (row != table.Length - 1) {
					ret += "\n";
				}
			}
			ret += "```";
			return ret;
		}
	}

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}
}
