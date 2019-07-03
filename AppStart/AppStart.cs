using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace RoosterBot.Automation {
	internal class AppStart {
		private static int Main(string[] args) {
			if (args[0] == "delay" && int.TryParse(args[1], out int result)) {
				Log($"Starting app in {result} milliseconds");
				Thread.Sleep(result);
			}
			Log("Starting app");

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../RoosterBot/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,
			};
			Process process = Process.Start(psi);

			Log("App started");

			Thread.Sleep(7000);
			if (process.HasExited) {
				Log("FAIL: App not running after 7 seconds, with exit code " + process.ExitCode);
				return process.ExitCode;
			} else {
				Log("OK: App running after 7 seconds");
				return 0;
			}
		}
		
		private static void Log(string message) {
			Console.WriteLine(DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern) + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", Environment.NewLine + DateTime.Now + " AppStart: " + message);
		}
	}
}
