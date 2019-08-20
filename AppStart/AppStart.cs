using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace RoosterBot.Automation {
	internal class AppStart {
		private static int Main(string[] args) {
			if (args.Length == 2 && args[0] == "delay" && int.TryParse(args[1], out int result)) {
				Log($"Starting app in {result} milliseconds");
				Thread.Sleep(result);
			}
			Log("Starting app");

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "../RoosterBot/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,
			};
			Process process = Process.Start(psi);
			
			Log("App started");

			// TODO: Move this code to a program/script running in the ValidateService phase, if possible
			Thread.Sleep(15000);
			if (process.HasExited) {
				Log("FAIL: App not running after 15 seconds, with exit code " + process.ExitCode);
				return process.ExitCode;
			} else {
				Log("OK: App running after 15 seconds");
				return 0;
			}
		}
		
		private static void Log(string message) {
			string nowString = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern);
			Console.WriteLine(nowString + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", nowString + " AppStart: " + message + Environment.NewLine);
		}
	}
}
