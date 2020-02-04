using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// The static class that takes care of logging in the entire program.
	/// </summary>
	public static class Logger {
		private static readonly object Lock = new object();
		private static readonly string LogPath = Path.Combine(Program.DataPath, "RoosterBot");
		private static readonly Dictionary<LogSeverity, string> SeverityStrings;

		static Logger() {
			// Keep the log from the previous launch as ".old.log"
			if (File.Exists(LogPath + ".log")) {
				if (File.Exists(LogPath + ".old.log")) {
					File.Delete(LogPath + ".old.log");
				}
				File.Move(LogPath + ".log", LogPath + ".old.log");
				LogPath += ".log";
				File.Create(LogPath).Dispose(); // File.Create automatically opens a stream to it, but we don't need that.
			} else {
				LogPath += ".log";
			}


			var severities = (LogSeverity[]) typeof(LogSeverity).GetEnumValues();
			int longestSeverity = severities.Max(sev => sev.ToString().Length);
			SeverityStrings = severities.ToDictionary(
				sev => sev,
				sev => " [" + sev.ToString().PadLeft(longestSeverity) + "] "
			);
		}

		/// <summary>
		/// Log a message at verbose level.
		/// </summary>
		public static void Verbose(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Verbose, tag, msg, e);
		}

		/// <summary>
		/// Log a message at debug level.
		/// </summary>
		public static void Debug(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Debug, tag, msg, e);
		}

		/// <summary>
		/// Log a message at informational level.
		/// </summary>
		public static void Info(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Info, tag, msg, e);
		}

		/// <summary>
		/// Log a message at warning level.
		/// </summary>
		public static void Warning(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Warning, tag, msg, e);
		}

		/// <summary>
		/// Log a message at error level.
		/// </summary>
		public static void Error(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Error, tag, msg, e);
		}

		/// <summary>
		/// Log a message at critical level.
		/// </summary>
		public static void Critical(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Critical, tag, msg, e);
		}

		private static void Log(LogSeverity severity, string tag, string msg, Exception? exception = null) {
			string loggedMessage = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern) + SeverityStrings[severity] + tag + " : " + msg;
			if (exception != null) {
				if (exception is FileLoadException) {
					loggedMessage += "\n" + exception.ToString();
				} else {
					loggedMessage += "\n" + exception.ToStringDemystified();
				}
			}
			lock (Lock) {
				Console.ForegroundColor = severity switch
				{
					LogSeverity.Verbose  => ConsoleColor.Gray,
					LogSeverity.Debug    => ConsoleColor.Gray,
					LogSeverity.Info     => ConsoleColor.White,
					LogSeverity.Warning  => ConsoleColor.Yellow,
					LogSeverity.Error    => ConsoleColor.Red,
					LogSeverity.Critical => ConsoleColor.Red,
					_ => ConsoleColor.White
				};
				Console.WriteLine(loggedMessage);
				Console.ForegroundColor = ConsoleColor.White;
				File.AppendAllText(LogPath, loggedMessage + Environment.NewLine);
			}
		}

		private enum LogSeverity {
			Verbose, Debug, Info, Warning, Error, Critical
		}
	}
}
