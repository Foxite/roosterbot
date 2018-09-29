using System;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public static class Logger {
		public static void Log(LogSeverity sev, string tag, string msg) {
			Log(new LogMessage(sev, tag, msg));
		}

		public static void Log(LogSeverity sev, string tag, string msg, Exception ex) {
			Log(new LogMessage(sev, tag, msg, ex));
		}
		
		public static void Log(LogMessage msg) {
			Console.WriteLine(DateTime.Now.ToUniversalTime() + " : [" + msg.Severity + "] " + msg.Source + " : " + msg.Message);
			if (msg.Exception != null) {
				Console.WriteLine(msg.Exception.ToString());
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
