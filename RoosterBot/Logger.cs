using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public static class Logger {
		public static readonly string LogPath;
		private static readonly object Lock;

		static Logger() {
			LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoosterBot", "RoosterBot");
			Lock = new object();
			
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

		public static void Verbose(string tag, string msg) {
			Log(LogSeverity.Verbose, tag, msg);
		}

		public static void Debug(string tag, string msg) {
			Log(LogSeverity.Debug, tag, msg);
		}

		public static void Info(string tag, string msg) {
			Log(LogSeverity.Info, tag, msg);
		}

		public static void Warning(string tag, string msg) {
			Log(LogSeverity.Warning, tag, msg);
		}

		public static void Error(string tag, string msg) {
			Log(LogSeverity.Error, tag, msg);
		}

		public static void Critical(string tag, string msg) {
			Log(LogSeverity.Critical, tag, msg);
		}

		public static void Log(LogSeverity sev, string tag, string msg) {
			Log(new LogMessage(sev, tag, msg));
		}

		public static void Log(LogSeverity sev, string tag, string msg, Exception ex) {
			Log(new LogMessage(sev, tag, msg, ex));
		}
		
		public static void Log(LogMessage msg) {
			string loggedMessage = DateTime.Now + " : [" + msg.Severity + "] " + msg.Source + " : " + msg.Message;
			if (msg.Exception != null) {
				loggedMessage += "\n" + msg.Exception.ToString();
			}
			Console.WriteLine(loggedMessage);
			lock (Lock) {
				File.AppendAllText(LogPath, loggedMessage + Environment.NewLine);
			}
		}

		/// <summary>
		/// This is called LogSync because it is not async, but Discord.NET requires a Log function that returns Task. None of the other functions here are async.
		/// </summary>
		public static Task LogSync(LogMessage msg) {
			Log(msg);
			return Task.CompletedTask;
		}
	}
}
