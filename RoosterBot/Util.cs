using System;
using System.Linq;

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
	}

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}
}
