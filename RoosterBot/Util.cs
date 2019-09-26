using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;

namespace RoosterBot {
	public static class Util {
		// TODO before deploying 2.0 make sure the bot can access this emote!
		public static readonly string ErrorPrefix = "<:rb_error:623935318814621717> ";
		public static readonly Random RNG = new Random();

		#region String utils
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
		/// Generate a table for use in a Discord message. Output will be formatted into a code block.
		/// Warning: This will glitch out when the table array is jagged.
		/// </summary>
		/// <param name="table">An array[row][column]. While it can be jagged, if it is jagged, this will not work properly.</param>
		/// <param name="includeHeaderSeperation">Include a line of '-'s after the first row.</param>
		/// <returns>A string that you can send directly into a chat message.</returns>
		public static string FormatTextTable(string[][] table) {
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
				invocationTasks[i] = (Task) invocationList[i].DynamicInvoke(parameters);
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
				await (Task) invocationList[i].DynamicInvoke(parameters);
			}
		}
		#endregion

		#region Discord utils
		public static async Task<bool> AddReaction(IUserMessage message, string unicode) {
			try {
				await message.AddReactionAsync(new Emoji(unicode));
				return true;
			} catch (HttpException) { // Permission denied
				return false;
			}
		}

		public static async Task<bool> RemoveReaction(IUserMessage message, string unicode, IUser botUser) {
			try {
				await message.RemoveReactionAsync(new Emoji(unicode), botUser);
				return true;
			} catch (HttpException) { // Permission denied
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
		public static IEnumerable<T> Add<T>(this IEnumerable<T> source, T item) {
			foreach (T current in source) {
				yield return current;
			}
			yield return item;
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
		#endregion
	}

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}
}
