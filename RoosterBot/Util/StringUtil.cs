using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RoosterBot {
	/// <summary>
	/// A static class containing several functions to manipulate strings or string arrays.
	/// </summary>
	public static class StringUtil {
		/// <summary>
		/// Capitalize the first character in a string.
		/// </summary>
		/// From https://stackoverflow.com/a/4405876/3141917
		public static string FirstCharToUpper(this string input) {
			if (input.Length == 0) {
				throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
			} else {
				return input[0].ToString().ToUpper() + input.Substring(1);
			}
		}

		/// <summary>
		/// Generate a table for use in a Discord message. Output will be formatted into a code block.
		/// Warning: This will glitch out when the table array is jagged.
		/// </summary>
		/// <param name="table">An array[row][column]. While it can be jagged, if it is jagged, this will not work properly.</param>
		/// <param name="maxColumnWidth">The maximum width for any column. Cells will be broken by word into lines if the cell length exceeds this value.</param>
		/// <returns>A string that you can send directly into a chat message.</returns>
		public static string FormatTextTable(IReadOnlyList<IReadOnlyList<string>> table, int? maxColumnWidth = null) {
			// Split cells into lines and determine row heights
			int[] rowHeights = new int[table.Count];
			var cellLines = new List<string>[table.Count][];

			for (int row = 0; row < table.Count; row++) {
				cellLines[row] = new List<string>[table[0].Count];
				for (int column = 0; column < table[0].Count; column++) {
					cellLines[row][column] = maxColumnWidth == null
						? new List<string>() { table[row][column] }
						: BreakStringIntoLines(table[row][column], maxColumnWidth.Value);
					rowHeights[row] = Math.Max(cellLines[row][column].Count, rowHeights[row]);
				}
			}

			// Determine column widths
			int[] columnWidths = new int[cellLines[0].Length];
			for (int column = 0; column < table[0].Count; column++) {
				columnWidths[column] = 1;
				for (int row = 0; row < table.Count; row++) {
					columnWidths[column] = Math.Max(columnWidths[column], cellLines[row][column].Max(str => str.Length));
				}
			}
			
			// Fill up unused space
			for (int row = 0; row < cellLines.Length; row++) {
				for (int column = 0; column < cellLines[row].Length; column++) {
					int addEmptyLines = rowHeights[row] - cellLines[row][column].Count;
					for (int line = 0; line < addEmptyLines; line++) {
						cellLines[row][column].Add(new string(' ', columnWidths[column]));
					}
				}
			}

			// Output
			string ret = "```\n";
			for (int row = 0; row < cellLines.Length; row++) {
				for (int rowLine = 0; rowLine < cellLines[row][0].Count; rowLine++) {
					for (int column = 0; column < cellLines[0].Length; column++) {
						List<string> lines = cellLines[row][column];
						ret += lines[rowLine].PadRight(columnWidths[column]);
						if (column == cellLines[0].Length - 1) {
							ret += "\n";
						} else {
							ret += " | ";
						}
					}
				}
			}
			ret += "```";
			return ret;
		}

		/// <summary>
		/// Break a string up into lines, with a maximum length of each line.
		/// The string will be split up into words by space, and each line will contain the maximum amount of words without exceeding the maximum line length.
		/// If a single word exceeds the maximum line length, it will be broken up and hyphenated.
		/// </summary>
		public static List<string> BreakStringIntoLines(string input, int maxLineLength) {
			string[] words = input.Split(' ', (char) 0x200B); // 200B == zero width space
			var lines = new List<string>();

			for (int i = 0; i < words.Length; i++) {
				string lastLine = lines.Count == 0 ? "" : lines[lines.Count - 1];
				void writeBackLastLine() {
					if (lines.Count == 0) {
						lines.Add(lastLine);
					} else {
						lines[lines.Count - 1] = lastLine;
					}
				}

				string word = words[i];

				if (lastLine.Length + 1 + word.Length <= maxLineLength) { // If it fits
					lastLine += (i == 0 ? "" : " ") + word;
					writeBackLastLine();
				} else if (word.Length > maxLineLength) { // If the word is longer than a line (-1 because we'll add a hyphen)
					// Break the word with a hyphen
					int breakPos = maxLineLength - 1;
					lastLine += (i == 0 ? "" : " ") + word.Substring(0, breakPos) + '-';
					writeBackLastLine();
					lines.Add("");
					words[i] = word[breakPos..];
					i--;
				} else { // If it does not fit in an existing line
					// Start a new line
					lines.Add(word);
				}
			}
			return lines;
		}

		/// <summary>
		/// Removes diacritics from characters: characters like â become a, etc.
		/// </summary>
		/// <remarks>
		/// https://stackoverflow.com/a/249126/3141917
		/// 
		/// This is apparently wrong, due to certain characters being replaced phonetically.
		/// </remarks>
		/// <returns></returns>
		public static string RemoveDiacritics(string text) {
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString) {
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the beginning of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the start of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimStart(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		/// <seealso cref="TrimEnd(string, string)"/>
		/// <seealso cref="TrimEnd(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		// Stolen from https://stackoverflow.com/a/4335913
		public static string TrimStart(this string target, string trimString) {
			if (string.IsNullOrEmpty(trimString)) return target;

			string result = target;
			while (result.StartsWith(trimString)) {
				result = result.Substring(trimString.Length);
			}

			return result;
		}

		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the end of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the end of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimStart(string, string)"/>
		/// <seealso cref="TrimStart(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		/// <seealso cref="TrimEnd(string, string)"/>
		// Also https://stackoverflow.com/a/4335913
		public static string TrimEnd(this string target, string trimString) {
			if (string.IsNullOrEmpty(trimString)) return target;

			string result = target;
			while (result.EndsWith(trimString)) {
				result = result.Substring(0, result.Length - trimString.Length);
			}

			return result;
		}

		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the end of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the end of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimStart(string, string)"/>
		/// <seealso cref="TrimEnd(string, string)"/>
		/// <seealso cref="TrimEnd(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		// Adapted from the previous 2 functions
		public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> target, ReadOnlySpan<char> trimString) {
			if (trimString.IsEmpty) return target;

			ReadOnlySpan<char> result = target;
			while (result.StartsWith(trimString)) {
				result = result.Slice(trimString.Length);
			}

			return result;
		}

		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the end of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the end of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimStart(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		/// <seealso cref="TrimStart(string, string)"/>
		/// <seealso cref="TrimEnd(string, string)"/>
		// Same
		public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> target, ReadOnlySpan<char> trimString) {
			if (trimString.IsEmpty) return target;

			ReadOnlySpan<char> result = target;
			while (result.EndsWith(trimString)) {
				result = result.Slice(0, result.Length - trimString.Length);
			}

			return result;
		}
	}
}
