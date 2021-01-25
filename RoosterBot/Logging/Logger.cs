using System;
using System.Collections.Generic;

namespace RoosterBot {
	/// <summary>
	/// The static class that takes care of logging in the entire program.
	/// </summary>
	public static class Logger {
		internal static class Tags {
			public const string RoosterBot = "RoosterBot";
			public const string Pipeline = "Pipeline";
		}

		private static readonly List<LogEndpoint> Endpoints = new List<LogEndpoint>();

		/// <summary>
		/// Register a <see cref="LogEndpoint"/> instance.
		/// </summary>
		/// <param name="endpoint"></param>
		public static void AddEndpoint(LogEndpoint endpoint) {
			Endpoints.Add(endpoint);
		}

		/// <summary>
		/// Log a message at verbose level.
		/// </summary>
		public static void Verbose(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Verbose, tag, msg, e);
		}

		/// <summary>
		/// Log a message at debug level.
		/// </summary>
		public static void Debug(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Debug, tag, msg, e);
		}

		/// <summary>
		/// Log a message at informational level.
		/// </summary>
		public static void Info(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Info, tag, msg, e);
		}

		/// <summary>
		/// Log a message at warning level.
		/// </summary>
		public static void Warning(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Warning, tag, msg, e);
		}

		/// <summary>
		/// Log a message at error level.
		/// </summary>
		public static void Error(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Error, tag, msg, e);
		}

		/// <summary>
		/// Log a message at critical level.
		/// </summary>
		public static void Critical(string tag, string msg, Exception? e = null) {
			Log(LogLevel.Critical, tag, msg, e);
		}

		/// <summary>
		/// Logs a message.
		/// </summary>
		public static void Log(LogLevel level, string tag, string msg, Exception? exception = null) {
			Log(new LogMessage(level, tag, msg, exception));
		}

		/// <summary>
		/// Logs a message.
		/// </summary>
		public static void Log(LogMessage message) {
			foreach (LogEndpoint endpoint in Endpoints) {
				endpoint.Log(message);
			}
		}
	}
}
