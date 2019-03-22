using System;
using System.Diagnostics;
using System.IO;

namespace RoosterBot.Automation {
	internal class AppStart {
		private const string Folder =
#if DEBUG
			"Debug"
#else
			"Release"
#endif
			;

		private static void Main(string[] args) {
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", DateTime.Now + " AppStart : Starting app");

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../../../RoosterBot/bin/" + Folder + "/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,
			};
			Process.Start(psi);

			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", DateTime.Now + " AppStart : App started");
		}
	}
}
