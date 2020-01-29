using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class LogHandler {

		public LogHandler(BaseSocketClient client) {
			client.Log += Log;
		}

		private Task Log(LogMessage msg) {
			Action<string, string, Exception?> logFunc = msg.Severity switch {
				LogSeverity.Verbose  => Logger.Verbose,
				LogSeverity.Debug    => Logger.Debug,
				LogSeverity.Info     => Logger.Info,
				LogSeverity.Warning  => Logger.Warning,
				LogSeverity.Error    => Logger.Error,
				LogSeverity.Critical => Logger.Critical,
				_                    => Logger.Info,
			};
			logFunc("Discord-" + msg.Source, msg.Message, msg.Exception);

			return Task.CompletedTask;
		}
	}
}
