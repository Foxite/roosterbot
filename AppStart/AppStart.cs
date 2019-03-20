using System;
using System.Diagnostics;
using System.IO;

namespace RoosterBot.Automation {
	internal class AppStart {
		private static void Main(string[] args) {
			Directory.CreateDirectory("C:/temp");
			using (StreamWriter append = File.AppendText("C:/temp/appstart.log")) {
				append.WriteLine(DateTime.Now.ToLongTimeString());
			}

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "../../../PipeServer/bin/Debug/PipeServer.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,

			};
			Process.Start(psi);
		}
	}
}
