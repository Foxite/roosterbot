using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class LogHandler {
		public LogHandler(BaseSocketClient client) {
			client.Log += Log;
		}

		private Task Log(Discord.LogMessage msg) {
			var level = (LogLevel) Enum.Parse(typeof(LogLevel), msg.Severity.ToString());
			Logger.Log(level, DiscordNetComponent.LogTag + "-" + msg.Source, msg.Message, msg.Exception);

			return Task.CompletedTask;
		}
	}
}
