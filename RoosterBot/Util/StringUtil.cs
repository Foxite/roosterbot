using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RoosterBot {
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
		public static string FormatTextTable(string[][] table, int maxColumnWidth = 20) {
			// Split cells into lines and determine row heights
			int[] rowHeights = new int[table.Length];
			List<string>[][] cellLines = new List<string>[table.Length][];
			for (int row = 0; row < table.Length; row++) {
				cellLines[row] = new List<string>[table[0].Length];
				for (int column = 0; column < table[0].Length; column++) {
					cellLines[row][column] = BreakStringIntoLines(table[row][column], maxColumnWidth);
					rowHeights[row] = Math.Max(cellLines[row][column].Count, rowHeights[row]);
				}
			}

			// Determine column widths
			int[] columnWidths = new int[cellLines[0].Length];
			for (int column = 0; column < table[0].Length; column++) {
				columnWidths[column] = 1;
				for (int row = 0; row < table.Length; row++) {
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

		public static List<string> BreakStringIntoLines(string input, int maxLineLength) {
			string[] words = input.Split(' ', (char) 0x200B); // 200B == zero width space
			List<string> lines = new List<string>();

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
					if (maxLineLength - lastLine.Length > 5) { // And at least the first 4 characters fit (excluding the space)
						// Break the word with a hyphen
						int breakPos = maxLineLength - lastLine.Length - 2;
						lastLine += ' ' + word.Substring(0, breakPos) + '-';
						writeBackLastLine();
						lines.Add(word.Substring(breakPos + 1));
					} else { // And less than 4 characters fit
						// Start a new line
						lines.Add(word);
					}
				}
			}
			return lines;
		}

		public static string EscapeString(string input) {
			List<(string replace, string with)> replacements = new List<(string replace, string with)>() {
				("\\", "\\\\"), // Needs to be done first
				("_", @"\_"),
				("*", @"\*"), // Also covers **, which need only their *first* side escaped, or all of the asterisks in *both* sides
				(">", @"\>"),
				(">>>", @"\>>>"),
				("<", @"\<"),
				("`", @"\`")
			};

			foreach ((string replace, string with) in replacements) {
				input = input.Replace(replace, with);
			}

			return input;
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
	}
}
