using System;
using System.IO;
using System.IO.Pipes;

namespace RoosterBot.Automation {
	internal class AppStop {
		private static void Main(string[] args) {
			using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "roosterbotStopPipe", PipeDirection.Out)) {
				try {
					pipeClient.Connect(1);
					using (StreamWriter sw = new StreamWriter(pipeClient)) {
						sw.WriteLine("stop");
					}
					Console.WriteLine("Process stopped.");
				} catch (TimeoutException) {
					Console.WriteLine("No process to stop.");
				}
			}
		}
	}
}
