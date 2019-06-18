using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;

namespace RoosterBot.Automation {
	internal class AppStop {
		private static void Main(string[] args) {
			Log("Stopping app");

			// Avoid nested `using` blocks because they may cause the outer object to be Dispose()d twice, causing an ObjectDisposedException.
			// https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2202-do-not-dispose-objects-multiple-times?view=vs-2019
			NamedPipeClientStream pipeClient = null;

			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotStopPipe", PipeDirection.Out);
				pipeClient.Connect(1);
				using (StreamWriter sw = new StreamWriter(pipeClient)) {
					pipeClient = null;
					sw.WriteLine("stop");
				}
				Log("Process stopped.");
			} catch (TimeoutException) {
				Log("No process to stop.");
			} finally {
				if (pipeClient != null) {
					pipeClient.Dispose();
				}
			}
		}

		private static void Log(string message) {
			Console.WriteLine(DateTime.Now + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", Environment.NewLine + DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern) + " AppStop : " + message);
		}
	}
}
