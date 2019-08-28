using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public static class Logger {
		private static readonly object s_Lock;
		private static readonly int s_LongestSeverity;

		public static readonly string LogPath;

		static Logger() {
			LogPath = Path.Combine(Program.DataPath, "RoosterBot");
			s_Lock = new object();

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

			// Non-generic array
			// Can't use a nice linq one-liner, unfortunately.
			// Not even indexers. Has to be difficult, obviously.
			Array severities = typeof(LogSeverity).GetEnumValues();
			bool first = true;
			foreach (object severity in severities) {
				if (first) {
					first = false;
					continue;
				} else {
					s_LongestSeverity = Math.Max(s_LongestSeverity, severity.ToString().Length);
				}
			}
		}

		public static void Verbose(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Verbose, tag, msg, e);
		}

		public static void Debug(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Debug, tag, msg, e);
		}

		public static void Info(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Info, tag, msg, e);
		}

		public static void Warning(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Warning, tag, msg, e);
		}

		public static void Error(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Error, tag, msg, e);
		}

		public static void Critical(string tag, string msg, Exception e = null) {
			Log(LogSeverity.Critical, tag, msg, e);
		}

		private static void Log(LogSeverity severity, string tag, string msg, Exception exception = null) {
			string severityStr = severity.ToString().PadLeft(s_LongestSeverity);

			string loggedMessage = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern)
								+ " [" + severityStr + "] " + tag + " : " + msg;
			if (exception != null) {
				if (exception is FileLoadException) {
					loggedMessage += "\n" + exception.ToString();
				} else {
					loggedMessage += "\n" + exception.ToStringDemystified();
				}
			}
			Console.WriteLine(loggedMessage);
			lock (s_Lock) {
				File.AppendAllText(LogPath, loggedMessage + Environment.NewLine);
			}
		}

		/// <summary>
		/// This is called LogSync because it is not async, but Discord.NET requires a Log function that returns Task. None of the other functions here are async.
		/// </summary>
		internal static Task LogSync(LogMessage msg) {
			Log(msg.Severity, msg.Source, msg.Message, msg.Exception);
			return Task.CompletedTask;
		}
	}
}
