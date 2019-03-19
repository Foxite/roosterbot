using System.Diagnostics;
using System.IO;

namespace RoosterBot.Automation {
	internal class AppStart {
		private static void Main(string[] args) {
			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../../../RoosterBot/bin/Debug/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,

			};
			Process.Start(psi);
		}
	}
}
