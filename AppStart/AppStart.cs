using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace RoosterBot.Automation {
	internal class AppStart {
		private static int Main(string[] args) {
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", DateTime.Now + " AppStart : Starting app");

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../../../RoosterBot/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,
			};
			Process process = Process.Start(psi);

			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", DateTime.Now + " AppStart : App started");

			Thread.Sleep(7000);
			if (process.HasExited) {
				return process.ExitCode;
			} else {
				return 0;
			}
		}
	}
}
