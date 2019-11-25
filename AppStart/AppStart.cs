using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RoosterBot.Automation {
	internal class AppStart {
		private Process m_Process;

		private static int Main(string[] args) {
			return new AppStart().MainAsync(args).GetAwaiter().GetResult();
		}

		private async Task<int> MainAsync(string[] args) {
			if (args.Length == 2 && args[0] == "delay" && int.TryParse(args[1], out int argResult)) {
				Log($"Starting app in {argResult} milliseconds");
				await Task.Delay(argResult);
			}
			Log("Starting app");

			ProcessStartInfo psi = new ProcessStartInfo() {
				FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "../RoosterBot/RoosterBot.exe"),
				CreateNoWindow = false,
				UseShellExecute = true,
			};
			m_Process = Process.Start(psi);

			Log("App started, monitoring");

			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken token = cts.Token;
			bool waitResult = await await Task.WhenAny(
				WaitForReport(token),
				WaitForExit(token)
			);
			cts.Cancel();
			cts.Dispose();

			if (waitResult) {
				Log("OK: App has reported Ready");
				return 0;
			} else {
				Log("FAIL: Process has exited with code " + m_Process.ExitCode);
				return m_Process.ExitCode;
			}
		}

		/// <returns>true if the process has sent a ready report</returns>
		private async Task<bool> WaitForReport(CancellationToken token) {
			using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("roosterbotReady", PipeDirection.In)) {
				await pipeServer.WaitForConnectionAsync(token);
				if (pipeServer.IsConnected) {
					using (StreamReader sr = new StreamReader(pipeServer)) {
						string input = sr.ReadLine();
						// This will fuck up if the received data is anything other than "ready", although that shouldn't happen (at the time of writing)
						if (input == "ready") {
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// This method returns on one of two conditions:
		/// - The token is cancelled. It returns true immediately.
		/// - The process exits. It returns false immediately.
		/// </summary>
		private async Task<bool> WaitForExit(CancellationToken token) {
			await m_Process.WaitForExitAsync(token);
			if (m_Process.HasExited) {
				return false;
			} else {
				return true;
			}
		}

		private static void Log(string message) {
			string nowString = DateTime.Now.ToString(DateTimeFormatInfo.CurrentInfo.UniversalSortableDateTimePattern);
			Console.WriteLine(nowString + " : " + message);
			File.AppendAllText("C:/ProgramData/RoosterBot/install.log", nowString + " AppStart: " + message + Environment.NewLine);
		}
	}
}
