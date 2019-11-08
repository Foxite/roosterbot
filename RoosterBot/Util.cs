using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace RoosterBot {
	public static class Util {
		public static readonly string Error = "<:error:636213609919283238> ";
		public static readonly string Success = "<:ok:636213617825546242> ";
		public static readonly string Warning = "<:warning:636213630114856962> ";
		public static readonly string Unknown = "<:unknown:636213624460935188> ";
		public static readonly Random RNG = new Random();

		#region String utils
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
		#endregion

		#region Async delegate
		private static void CheckAsyncDelegate(Delegate asyncEvent, object[] parameters) {
			if (asyncEvent.Method.ReturnType != typeof(Task)) {
				throw new ArgumentException($"{nameof(asyncEvent)} must return Task", nameof(asyncEvent));
			}

			System.Reflection.ParameterInfo[] delegateParams = asyncEvent.Method.GetParameters();
			for (int i = 0; i < delegateParams.Length; i++) {
				if (!delegateParams[i].ParameterType.IsAssignableFrom(parameters[i].GetType())) {
					throw new ArgumentException($"Given parameter {i} must be assignable to the equivalent delegate parameter.", nameof(parameters));
				}
			}
		}

		/// <summary>
		/// Invokes an async delegate in such a way that the invocations run at the same time.
		/// </summary>
		public static async Task InvokeAsyncEventConcurrent(Delegate asyncEvent, params object[] parameters) {
			CheckAsyncDelegate(asyncEvent, parameters);

			Delegate[] invocationList = asyncEvent.GetInvocationList();
			Task[] invocationTasks = new Task[invocationList.Length];

			for (int i = 0; i < invocationList.Length; i++) {
				invocationTasks[i] = (Task) invocationList[i].DynamicInvoke(parameters)!;
			}

			await Task.WhenAll(invocationTasks);
		}

		/// <summary>
		/// Invokes an async delegate in such a way that the invocations run one by one.
		/// </summary>
		public static async Task InvokeAsyncEventSequential(Delegate asyncEvent, params object[] parameters) {
			CheckAsyncDelegate(asyncEvent, parameters);

			Delegate[] invocationList = asyncEvent.GetInvocationList();

			for (int i = 0; i < invocationList.Length; i++) {
				await (Task) invocationList[i].DynamicInvoke(parameters)!;
			}
		}
		#endregion

		#region Discord utils
		public static IReadOnlyCollection<IGuild> GetMutualGuilds(this IUser user) {
			if (user is SocketUser socketUser) {
				return socketUser.MutualGuilds;
			} else {
				return Array.Empty<IGuild>();
			}
		}

		public static async Task<bool> AddReaction(IUserMessage message, string unicode) {
			try {
				await message.AddReactionAsync(new Emoji(unicode));
				return true;
			} catch (HttpException e) { // Permission denied
				Logger.Warning("Util", "Attempted to add a reaction to a message, but this failed", e);
				return false;
			}
		}

		public static async Task<bool> RemoveReaction(IUserMessage message, string unicode, IUser botUser) {
			try {
				await message.RemoveReactionAsync(new Emoji(unicode), botUser);
				return true;
			} catch (HttpException e) { // Permission denied
				Logger.Warning("Util", "Attempted to add a reaction to a message, but this failed", e);
				return false;
			}
		}

		public static async Task DeleteAll(IMessageChannel channel, IEnumerable<IUserMessage> messages) {
			if (channel is ITextChannel textChannel) {
				await textChannel.DeleteMessagesAsync(messages);
			} else {
				// No idea what kind of non-text MessageChannel there are, but at least they support non-bulk deletion.
				foreach (IUserMessage message in messages) {
					await message.DeleteAsync();
				}
			}
		}

		/// <summary>
		/// Given a set of messages that we sent, this will delete all messages except the first, and modify the first according to <paramref name="message"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="responses"></param>
		/// <param name="append">If true, this will append <paramref name="message"/> to the first response, otherwise this will overwrite the contents of that message.</param>
		/// <returns></returns>
		public static async Task<IUserMessage> ModifyResponsesIntoSingle(string message, IEnumerable<IUserMessage> responses, bool append) {
			IUserMessage singleResponse = responses.First();
			IEnumerable<IUserMessage> extraMessages = responses.Skip(1);

			if (extraMessages.Any()) {
				await DeleteAll(singleResponse.Channel, extraMessages);
			}

			await singleResponse.ModifyAsync((msgProps) => {
				if (append) {
					msgProps.Content += "\n\n" + message;
				} else {
					msgProps.Content = message;
				}
			});
			return singleResponse;
		}
		#endregion

		#region LINQ
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class {
			foreach (T? item in enumerable) {
				if (!(item is null)) {
					yield return item;
				}
			}
		}
		
		/// <summary>
		/// Returns the params list as an IEnumerable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static IEnumerable<T> Pack<T>(params T[] items) {
			for (int i = 0; i < items.Length; i++) {
				yield return items[i];
			}
		}

		public static IEnumerable<LinkedListNode<T>> GetNodes<T>(this LinkedList<T> list) {
			if (list.Count > 0) {
				LinkedListNode<T>? node = list.First;
				while (node != null) {
					yield return node;
					node = node.Next;
				}
			}
		}

		/// <summary>
		/// Adds all items that match a predicate into a separate IEnumerable<T>, and returns all items that did not pass the predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="moveInto"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static IEnumerable<T> Divide<T>(this IEnumerable<T> source, out IEnumerable<T> moveInto, Func<T, bool> predicate) {
			List<T> outA = new List<T>();
			List<T> outB = new List<T>();

			foreach (T item in source) {
				List<T> outInto = predicate(item) ? outB : outA;
				outInto.Add(item);
			}
			moveInto = outB;
			return outA;
		}

		public static bool HasRole(this IGuildUser user, ulong roleId) {
			return user.RoleIds.Any(id => id == roleId);
		}
		#endregion
	}

	public class ReturnValue<T> {
		private readonly T m_Value;

		public bool Success { get; }
		public T Value => Success ? m_Value : throw new InvalidOperationException("Can't get the result of an unsuccessful operation");

		public ReturnValue() {
			Success = false;
			m_Value = default!;
		}

		public ReturnValue(T value) {
			Success = true;
			m_Value = value;
		}
	}
}
