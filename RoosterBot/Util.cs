using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

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

		public static string ResolveString(ComponentBase component, string str) {
			if (str.StartsWith("#")) {
				return component.GetStringResource(str.Substring(1));
			} else {
				return str;
			}
		}

		private static void CheckAsyncTelegate(Delegate asyncEvent, object[] parameters) {
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
			CheckAsyncTelegate(asyncEvent, parameters);

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
			CheckAsyncTelegate(asyncEvent, parameters);

			Delegate[] invocationList = asyncEvent.GetInvocationList();

			for (int i = 0; i < invocationList.Length; i++) {
				await (Task) invocationList[i].DynamicInvoke(parameters);
			}
		}

		/// <summary>
		/// Gets all guilds that the Bot user is in, that the given IUser is also in.
		/// </summary>
		public static async Task<IReadOnlyCollection<IGuild>> GetCommonGuildsAsync(IDiscordClient client, IUser user) {
			IReadOnlyCollection<IGuild> allGuilds = await client.GetGuildsAsync();
			List<IGuild> commonGuilds = new List<IGuild>();

			foreach (IGuild guild in allGuilds) {
				IReadOnlyCollection<IGuildUser> guildUsers = await guild.GetUsersAsync();
				if (guildUsers.Any(guilduser => guilduser.Id == user.Id)) {
					commonGuilds.Add(guild);
				}
			}

			return commonGuilds.AsReadOnly();
		}
	}

	public class ReturnValue<T> {
		public bool Success;
		public T Value;
	}

	public class CachedData<T> {
		private T m_Value = default(T);
		private bool m_IsKnown = false;

		public bool IsKnown {
			get {
				return m_IsKnown;
			}
			set {
				m_IsKnown = value;
				if (!m_IsKnown) {
					m_Value = default(T);
				}
			}
		}

		public T Value {
			get {
				if (IsKnown) {
					return m_Value;
				} else {
					throw new InvalidOperationException("Cached data is not known.");
				}
			}
			set {
				m_Value = value;
				IsKnown = true;
			}
		}
	}
}
