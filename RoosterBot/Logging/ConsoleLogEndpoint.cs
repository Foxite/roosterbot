using System;

namespace RoosterBot {
	internal class ConsoleLogEndpoint : LogEndpoint {
		public override void Log(LogMessage message) {
			string msg = FormatMessage(message);

			Console.ForegroundColor = message.Level switch
			{
				LogLevel.Verbose => ConsoleColor.Gray,
				LogLevel.Debug => ConsoleColor.Gray,
				LogLevel.Info => ConsoleColor.White,
				LogLevel.Warning => ConsoleColor.Yellow,
				LogLevel.Error => ConsoleColor.Red,
				LogLevel.Critical => ConsoleColor.Red,
				_ => ConsoleColor.White
			};
			Console.WriteLine(msg);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
