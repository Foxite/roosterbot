using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;

namespace RoosterBot.Automation {
	internal class AppStop {
		private static void Main(string[] args) {
			Log("Stopping app");
			
			using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "roosterbotStopPipe", PipeDirection.Out)) {
				try {
					pipeClient.Connect(1);
					using (StreamWriter sw = new StreamWriter(pipeClient)) {
						sw.WriteLine("stop");
					}
					Log("Process stopped.");
				} catch (TimeoutException) {
					Log("No process to stop.");
				}
			}
		}

		private static void Log(string message) {
			Console.WriteLine(DateTime.Now + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", Environment.NewLine + DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern) + " AppStop : " + message);
		}
	}
}
