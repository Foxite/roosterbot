using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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

		#region GetRange(T[])
		public static T[] GetRange<T>(this T[] source, int start, int count) {
			T[] ret = new T[count];
			Array.Copy(source, start, ret, 0, count);
			return ret;
		}

		public static T[] GetRange<T>(this T[] source, int start) {
			return GetRange(source, start, source.Length - start);
		}
		#endregion

		#region Levenshtein
		/// <summary>
		/// Returns the Levenshtein distance from {source} to {target}.
		/// From https://stackoverflow.com/a/6944095/3141917
		/// </summary>
		public static int Levenshtein(string source, string target) {
			if (string.IsNullOrEmpty(source)) {
				if (string.IsNullOrEmpty(target))
					return 0;
				return target.Length;
			}

			if (string.IsNullOrEmpty(target)) {
				return source.Length;
			}

			int n = source.Length;
			int m = target.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++)
				;
			for (int j = 1; j <= m; d[0, j] = j++)
				;

			for (int i = 1; i <= n; i++) {
				for (int j = 1; j <= m; j++) {
					int cost = (target[j - 1] == source[i - 1]) ? 0 : Math.Min(n - i, m - j);
					int min1 = d[i - 1, j] + n - i;
					int min2 = d[i, j - 1] + m - j;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
		#endregion

		#region Longest Common Subsequence
		private class Cell {
			public enum Directions {
				None,
				Up,
				Left,
				/// <summary>
				/// Refers to Top,Left
				/// </summary>
				Diagonal
			}

			public int LCS { get; set; }
			public Directions Direction { get; set; }
			//backward reference so that we can backtrack
			public Cell From { get; set; }
			/// <summary>
			/// C will only have values for row 0 and column 0
			/// </summary>
			public char C { get; set; }
			public int X { get; set; }
			public int Y { get; set; }

			public Directions GetDirection(Cell c) {
				if (c.X == X - 1 && c.Y == Y - 1) {
					return Directions.Diagonal;
				} else if (c.X == X && c.Y == Y - 1) {
					return Directions.Up;
				} else if (c.X == X - 1 && c.Y == Y) {
					return Directions.Left;
				} else {
					return Directions.None;
				}
			}
		}

		public static string GetLongestCommonSubsequence(string x, string y) {
			// columns and rows + 1 because we need to create an empty cell at [0,0]
			int columns = x.Length + 1;
			int rows = y.Length + 1;

			Cell[] cells = new Cell[rows * columns];
			cells[0] = new Cell();

			//initialise column 0 cells all to '0'
			for (int c = 1; c < columns; c++) {
				cells[c] = new Cell() { X = c, Y = 0, C = x[c - 1] };
			}

			//initialise row 0 cells all to '0'
			for (int r = 1; r < rows; r++) {
				cells[r * columns] = new Cell() { X = 0, Y = r, C = y[r - 1] };
			}

			//up till now are initialisation steps. the LCS algo starts here

			for (int r = 1; r < rows; r++) {
				for (int c = 1; c < columns; c++) {
					var cell = new Cell() { X = c, Y = r };

					var thisrow = cells[r * columns];
					var thiscol = cells[c];

					//compare row and column, if they have the same character, select diagonal cell's LCS
					if (thisrow.C == thiscol.C) {
						var diagcell = cells[(r - 1) * columns + c - 1];
						cell.LCS = diagcell.LCS + 1;
						cell.From = diagcell;
					} else {
						var uppercell = cells[(r - 1) * columns + c];
						var leftcell = cells[r * columns + c - 1];

						//take the larger LCS, if not use the upper cell's LCS
						if (leftcell.LCS > uppercell.LCS) {
							cell.LCS = leftcell.LCS;
							cell.From = leftcell;
						} else {
							cell.LCS = uppercell.LCS;
							cell.From = uppercell;
						}
					}

					cells[r * columns + c] = cell;
				}
			}

			//start backtracking
			//we will be getting characters in reverse, so we will use reverse
			Stack<char> stack = new Stack<char>();

			//last cell i.e bottom right most
			Cell curr = cells[rows * columns - 1];
			int length = curr.LCS;
			while (curr.From != null) {
				var from = curr.From;
				var dir = curr.GetDirection(from);
				if (dir == Cell.Directions.Diagonal) {
					var c = cells[curr.X].C;
					stack.Push(c);
				}
				curr = from;
			}

			StringBuilder sbLcs = new StringBuilder();
			for (int i = 0; i < length; i++) {
				sbLcs.Append(stack.Pop());
			}
			return sbLcs.ToString();
		}
		#endregion

		/// <summary>
		/// Finds a mention in a string, then returns the ID in that mention.
		/// </summary>
		/// <param name="startIndex">The index where the mention starts.</param>
		/// <param name="endIndex">The index where the mention ends.</param>
		/// <returns>The ID in the mention, or null if there is no valid mention.</returns>
		public static ulong? ExtractIDFromMentionString(string search) {
			int startIndex = search.IndexOf("<@");
			if (startIndex != -1) {
				if (search[startIndex + 2] == '!') {
					startIndex++;
				}
				int endIndex = search.IndexOf(">", startIndex);
				return ulong.Parse(search.Substring(startIndex + 2, endIndex - startIndex - 2));
			}
			return null;
		}

		private static string GetModuleSignature(this ModuleInfo module) {
			string ret = module.Name;
			if (!string.IsNullOrEmpty(module.Group)) {
				ret = $"{module.Group} {ret}";
			}

			if (module.IsSubmodule) {
				return $"{GetModuleSignature(module.Parent)} {ret}";
			} else {
				return ret;
			}
		}

		public static string GetCommandSignature(this CommandInfo command) {
			string ret = command.Name;

			bool notFirst = false;
			foreach (ParameterInfo param in command.Parameters) {
				ret += param.Type.Name + " " + param.Name;
				if (notFirst) {
					ret += ", ";
				}
				notFirst = true;
			}

			string moduleSig = command.Module.GetModuleSignature();
			if (!string.IsNullOrEmpty(moduleSig)) {
				ret = moduleSig + " " + ret;
			}

			return ret;
		}
	}

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}
}
