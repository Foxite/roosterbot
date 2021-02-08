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
		internal const string LogTag = "Console";

		private readonly CancellationTokenSource m_CTS;

		public override Version ComponentVersion => new Version(1, 0, 0);

		internal static ConsoleComponent Instance { get; private set; } = null!;
		internal ConsoleUser TheConsoleUser { get; } = new ConsoleUser(1, "User");
		internal ConsoleUser ConsoleBotUser { get; } = new ConsoleUser(2, "RoosterBot");
		internal ConsoleChannel TheConsoleChannel { get; } = new ConsoleChannel();

		public override string PlatformName => "Console";

		public override Type SnowflakeIdType => typeof(ulong);

		public ConsoleComponent() {
			Instance = this;
			m_CTS = new CancellationTokenSource();
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			var userParser = new UserParser();
			commandService.AddTypeParser(userParser);
			commandService.GetPlatformSpecificParser<IUser>().RegisterParser(this, new ConversionParser<ConsoleUser, IUser>("Console user", userParser, user => user));

			var emotes = services.GetRequiredService<EmoteService>();
			emotes.RegisterEmote(this, "Info",    new Emoji("i "));
			emotes.RegisterEmote(this, "Error",   new Emoji("x "));
			emotes.RegisterEmote(this, "Success", new Emoji("v "));
			emotes.RegisterEmote(this, "Warning", new Emoji("! "));
			emotes.RegisterEmote(this, "Unknown", new Emoji("? "));
		}

		protected override void Connect(IServiceProvider services) {
			UserConfigService ucs = services.GetRequiredService<UserConfigService>();

			Process.Start(new ProcessStartInfo() {
				FileName = @"..\..\RoosterBot.Console.App\bin\Debug\netcoreapp3.0\RoosterBot.Console.App.exe",
				CreateNoWindow = false,
				UseShellExecute = true
			});

			ChannelConfigService ccs = services.GetRequiredService<ChannelConfigService>();
			RoosterCommandService commandService = services.GetRequiredService<RoosterCommandService>();

			_ = Task.Run(async () => {
				NamedPipeServerStream? pipeServer = null;
				StreamReader? sr = null;
				bool wasConnected = false;
				try {
					pipeServer = new NamedPipeServerStream("roosterBotConsolePipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough);
					await pipeServer.WaitForConnectionAsync();
					Logger.Info(LogTag, "Console interface connected");

					sr = new StreamReader(pipeServer, Encoding.UTF8, true, 2047, true);
					while (pipeServer.IsConnected && !m_CTS.IsCancellationRequested) {
						wasConnected = true;
						string? input = await sr.ReadLineAsync();

						if (input == null) {
							Logger.Info(LogTag, "Console handler disconnected");
							return;
						}

						var consoleMessage = new ConsoleMessage(input, false);
						TheConsoleChannel.m_Messages.Add(consoleMessage);
						/* TODO Use console context and adapters
						await Program.Instance.CommandHandler.ExecuteCommandAsync(consoleMessage.Content, new RoosterCommandContext(services, this, consoleMessage,
							await ucs.GetConfigAsync(TheConsoleUser.GetReference()),
							await ccs.GetConfigAsync(TheConsoleChannel.GetReference())
						));
						*/
					}
				} catch (Exception e) {
					if (e is IOException && pipeServer != null && !pipeServer.IsConnected && wasConnected) {
						Logger.Info(LogTag, "Console interface disconnected");
					} else {
						Logger.Error(LogTag, "Console handler caught an exception: ", e);
					}
				} finally {
					Logger.Debug(LogTag, "Console handler exiting");
					if (m_CTS.IsCancellationRequested) {
						m_CTS.Dispose();
					}
					if (sr != null) {
						sr.Dispose();
						sr = null;
					}
					if (pipeServer != null) {
						pipeServer.Dispose();
						pipeServer = null;
					}

				}
			});
		}

		protected override void Disconnect() {
			m_CTS.Cancel();
		}

		public override object GetSnowflakeIdFromString(string input) => ulong.Parse(input);

		protected override void Dispose(bool disposing) {
			if (disposing) {
				m_CTS.Cancel();
			}
		}
	}
}
