﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RoosterBot {
	public static class Logger {
		private static readonly object Lock = new object();
		private static readonly int LongestSeverity = ((LogSeverity[]) typeof(LogSeverity).GetEnumValues()).Max(sev => sev.ToString().Length);

		public static readonly string LogPath = Path.Combine(Program.DataPath, "RoosterBot");

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
		}

		public static void Verbose(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Verbose, tag, msg, e);
		}

		public static void Debug(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Debug, tag, msg, e);
		}

		public static void Info(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Info, tag, msg, e);
		}

		public static void Warning(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Warning, tag, msg, e);
		}

		public static void Error(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Error, tag, msg, e);
		}

		public static void Critical(string tag, string msg, Exception? e = null) {
			Log(LogSeverity.Critical, tag, msg, e);
		}

		private static void Log(LogSeverity severity, string tag, string msg, Exception? exception = null) {
			string severityStr = severity.ToString().PadLeft(LongestSeverity);
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
			lock (Lock) {
				File.AppendAllText(LogPath, loggedMessage + Environment.NewLine);
			}
		}

		private enum LogSeverity {
			Verbose, Debug, Info, Warning, Error, Critical
		}
	}
}
