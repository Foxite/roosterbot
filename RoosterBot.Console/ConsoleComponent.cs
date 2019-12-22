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
		internal ConsoleUser TheConsoleUser { get; } = new ConsoleUser(1, "User");
		internal ConsoleUser ConsoleBotUser { get; } = new ConsoleUser(2, "RoosterBot");
		internal ConsoleChannel TheConsoleChannel { get; } = new ConsoleChannel();

		public ConsoleComponent() {
			Instance = this;
			m_CTS = new CancellationTokenSource();
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			UserConfigService ucs = services.GetService<UserConfigService>();
			ChannelConfigService ccs = services.GetService<ChannelConfigService>();

			_ = Task.Run(async () => {
				try {
					using var pipeServer = new NamedPipeServerStream("roosterBotConsolePipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
					Logger.Debug("Console", "AAA");
					pipeServer.WaitForConnection();
					Logger.Debug("Console", "BBB");

					using var sw = new StreamWriter(pipeServer, Encoding.UTF8, 2047, true);
					using var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 2047, true);
					while (pipeServer.IsConnected && !m_CTS.IsCancellationRequested) {
						Logger.Debug("Console", "CCC");
						string input = sr.ReadLine()!;
						Logger.Debug("Console", "DDD");

						var consoleMessage = new ConsoleMessage(input, false);
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
						sw.WriteLine(resultString);
						sw.Flush();
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
