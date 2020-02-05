using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// An endpoint for the <see cref="Logger"/> class. All messages logged using <see cref="Logger"/> will be passed to every registered instance of this class.
	/// </summary>
	public abstract class LogEndpoint {
		private static readonly Dictionary<LogLevel, string> LevelStrings;

		static LogEndpoint() {
			var levels = (LogLevel[]) typeof(LogLevel).GetEnumValues();
			int longestLevelLength = levels.Max(sev => sev.ToString().Length);
			LevelStrings = levels.ToDictionary(
				sev => sev,
				sev => "[" + sev.ToString().PadLeft(longestLevelLength) + "] "
			);
		}

		/// <summary>
		/// Formats a log message. See the examples for details on the output.
		/// </summary>
		/// <example>
		/// If you call this function on the 4th of February, in 2020, at exactly 16:00:00:
		/// <code>
		/// FormatMessage(new LogMessage(LogLevel.Info, "Example", "This is a test message."))
		/// </code>
		/// Returns:
		/// <code>
		/// 2020-02-04 16:00:00Z [    Info] Example : This is a test message.
		/// </code>
		/// 
		/// <code>
		/// FormatMessage(new LogMessage(LogLevel.Error, "Source", "I done goofed up"))
		/// </code>
		/// Returns:
		/// <code>
		/// 2020-02-04 16:00:00Z [Critical] Source : I done goofed up
		/// </code>
		/// 
		/// If the <see cref="LogMessage"/> includes an exception, it will be added to the result using <see cref="ExceptionExtentions.ToStringDemystified(Exception)"/>;
		/// </example>
		protected static string FormatMessage(LogMessage msg, bool includeTime = true) {
			string loggedMessage = LevelStrings[msg.Level] + msg.Tag + " : " + msg.Message;

			if (includeTime) {
				loggedMessage = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern) + " " + loggedMessage;
			}

			if (msg.Exception != null) {
				if (msg.Exception is FileLoadException) {
					loggedMessage += Environment.NewLine + msg.Exception.ToString();
				} else {
					loggedMessage += Environment.NewLine + msg.Exception.ToStringDemystified();
				}
			}
			return loggedMessage;
		}

		/// <summary>
		/// Send a <see cref="LogMessage"/> to this endpoint.
		/// </summary>
		public abstract void Log(LogMessage message);
	}
}