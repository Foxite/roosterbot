using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Automation {
	internal class AppStop {
		private static void Main() {
			Log("Stopping app");

			// Avoid nested `using` blocks because they may cause the outer object to be Dispose()d twice, causing an ObjectDisposedException.
			// https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2202-do-not-dispose-objects-multiple-times?view=vs-2019
			NamedPipeClientStream pipeClient = null;

			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotStopPipe", PipeDirection.Out);
				pipeClient.Connect(1);

				Process[] processes = Process.GetProcessesByName("RoosterBot");
				using (StreamWriter sw = new StreamWriter(pipeClient)) {
					pipeClient = null;
					sw.WriteLine("stop");
				}
				Task.WaitAny(processes.Select((process) => {
					process.WaitForExit();
					return Task.CompletedTask;
				}).ToArray());
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
			string nowString = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern);
			Console.WriteLine(nowString + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", nowString + " AppStop : " + message + Environment.NewLine);
		}
	}
}
