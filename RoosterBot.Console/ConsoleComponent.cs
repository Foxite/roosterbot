using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Console {
	public class ConsoleComponent : Component {
		private readonly CancellationTokenSource m_CTS;

		public override Version ComponentVersion => new Version(0, 1, 0);

		internal static ConsoleComponent Instance { get; private set; } = null!;
		internal ConsoleUser TheConsoleUser { get; } = new ConsoleUser();
		internal ConsoleChannel TheConsoleChannel { get; } = new ConsoleChannel();

		public ConsoleComponent() {
			Instance = this;
			m_CTS = new CancellationTokenSource();
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			UserConfigService ucs = services.GetService<UserConfigService>();
			ChannelConfigService ccs = services.GetService<ChannelConfigService>();

			/*/
			Process.Start(new ProcessStartInfo() {
				FileName = @"C:\Development\RoosterBot\RoosterBot.Console.App\bin\Debug\netcoreapp3.0\RoosterBot.Console.App.exe",
				CreateNoWindow = false,
				UseShellExecute = true,
			});//*/

			_ = Task.Run(async () => {
				try {
					using var pipeServer = new NamedPipeServerStream("roosterBotConsolePipe", PipeDirection.InOut);
					using var sw = new StreamWriter(pipeServer, Encoding.UTF8, 2047, true);
					using var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 2047, true);
					Logger.Debug("Console", "AAA");
					await pipeServer.WaitForConnectionAsync();
					Logger.Debug("Console", "BBB");

					var buffer = new Memory<char>(new char[1024]);
					while (!m_CTS.IsCancellationRequested) {
						Logger.Debug("Console", "CCC");
						await sr.ReadAsync(buffer, m_CTS.Token);
						Logger.Debug("Console", "DDD");

						var consoleMessage = new ConsoleMessage(buffer.ToString(), false);
						TheConsoleChannel.m_Messages.Add(consoleMessage);
						Logger.Debug("Console", "EEE");
						var result = await commandService.ExecuteAsync(consoleMessage.Content, new RoosterCommandContext(
							consoleMessage,
							await ucs.GetConfigAsync(TheConsoleUser),
							await ccs.GetConfigAsync(TheConsoleChannel),
							services
						));
						Logger.Debug("Console", "FFF");
						string resultString = result.ToString()!;
						await TheConsoleChannel.SendMessageAsync(resultString);
						Logger.Debug("Console", "GGG");
						await sw.WriteAsync(resultString);
						Logger.Debug("Console", "HHH");
					}
				} catch (Exception e) {
					Logger.Error("Console", "XXX", e);
				} finally {
					Logger.Debug("Console", "III");
					if (m_CTS.IsCancellationRequested) {
						Logger.Debug("Console", "JJJ");
						m_CTS.Dispose();
					}
				}
			});
			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				m_CTS.Cancel();
			}
		}
	}
}
