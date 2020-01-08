using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Console {
	public class ConsoleComponent : PlatformComponent {
		private readonly CancellationTokenSource m_CTS;

		public override Version ComponentVersion => new Version(0, 1, 0);

		internal static ConsoleComponent Instance { get; private set; } = null!;
		internal ConsoleUser TheConsoleUser { get; } = new ConsoleUser(1, "User");
		internal ConsoleUser ConsoleBotUser { get; } = new ConsoleUser(2, "RoosterBot");
		internal ConsoleChannel TheConsoleChannel { get; } = new ConsoleChannel();

		public override string PlatformName => "Console";

		public ConsoleComponent() {
			Instance = this;
			m_CTS = new CancellationTokenSource();
		}

		protected override Task ConnectAsync(IServiceProvider services) {
			Process.Start(new ProcessStartInfo() {
				FileName = @"C:\Development\RoosterBot\RoosterBot.Console.App\bin\Debug\netcoreapp3.0\RoosterBot.Console.App.exe",
				CreateNoWindow = false,
				UseShellExecute = true
			});

			UserConfigService ucs = services.GetService<UserConfigService>();
			ChannelConfigService ccs = services.GetService<ChannelConfigService>();
			RoosterCommandService commandService = services.GetService<RoosterCommandService>();

			_ = Task.Run(async () => {
				try {
					using var pipeServer = new NamedPipeServerStream("roosterBotConsolePipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
					pipeServer.WaitForConnection();
					Logger.Info("Console", "Console interface connected");

					using var sw = new StreamWriter(pipeServer, Encoding.UTF8, 2047, true);
					using var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 2047, true);
					while (pipeServer.IsConnected && !m_CTS.IsCancellationRequested) {
						string input = sr.ReadLine()!;

						var consoleMessage = new ConsoleMessage(input, false);
						TheConsoleChannel.m_Messages.Add(consoleMessage);
						var result = await commandService.ExecuteAsync(consoleMessage.Content, new RoosterCommandContext(
							consoleMessage,
							await ucs.GetConfigAsync(TheConsoleUser),
							await ccs.GetConfigAsync(TheConsoleChannel),
							services
						));
						string resultString = result.ToString()!;
						await TheConsoleChannel.SendMessageAsync(resultString);
						sw.Write(resultString + '\0');
						sw.Flush();
					}
				} catch (Exception e) {
					Logger.Error("Console", "Console handler caught an exception: ", e);
				} finally {
					Logger.Debug("Console", "Console handler exiting");
					if (m_CTS.IsCancellationRequested) {
						m_CTS.Dispose();
					}
				}
			});
			return Task.CompletedTask;
		}

		protected override Task DisconnectAsync() {
			m_CTS.Cancel();
			return Task.CompletedTask;
		}

		public override object GetSnowflakeIdFromString(string input) => ulong.Parse(input);

		protected override void Dispose(bool disposing) {
			if (disposing) {
				m_CTS.Cancel();
			}
		}
	}
}
