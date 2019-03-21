using System;
using System.IO;
using System.IO.Pipes;

namespace RoosterBot.Automation {
	internal class AppStop {
		private static void Main(string[] args) {
			File.AppendAllText("C:/RoosterBot/install.log", DateTime.Now + " AppStop : Stopping app");

			bool stopped = false;
			using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "roosterbotStopPipe", PipeDirection.Out)) {
				try {
					pipeClient.Connect(1);
					using (StreamWriter sw = new StreamWriter(pipeClient)) {
						sw.WriteLine("stop");
					}
					Console.WriteLine("Process stopped.");
					stopped = true;
				} catch (TimeoutException) {
					Console.WriteLine("No process to stop.");
				}
			}
			if (stopped) {
				File.AppendAllText("C:/RoosterBot/install.log", DateTime.Now + " AppStop : App stopped");
			} else {
				File.AppendAllText("C:/RoosterBot/install.log", DateTime.Now + " AppStop : App not running");
			}
		}
	}
}
