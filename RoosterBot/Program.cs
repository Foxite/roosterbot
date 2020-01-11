// TODO (refactor) The program currently throws a ton of boneheaded exceptions: https://blogs.msdn.microsoft.com/ericlippert/2008/09/10/vexing-exceptions/
// We really should avoid that wherever possible. Instead of trying to index an array and letting the caller deal with a mysterious "IndexOutOfRangeException",
//  we should catch that before doing anything else and throw an ArgumentException with an actual explanation.

// TODO (refactor) Add docstrings to all public things. There's a way to have VS raise a warning for missing docstrings; Qmmands has this enabled, so go steal it from them.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";

#nullable disable
		public static Program Instance { get; private set; }
#nullable restore

		public ComponentManager Components { get; private set; }
		public CommandExecutionHandler CommandHandler { get; private set; }

		private bool m_ShutDown;
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Log crash and exit")]
		private static int Main(string[] args) {
			Console.CancelKeyPress += (o, e) => {
				e.Cancel = true;
				Console.WriteLine("Use Ctrl-Q to stop the program, or force-quit this window if it is not responding.");
			};

			if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
				Console.WriteLine("There is already a process named RoosterBot running. There cannot be more than one instance of the bot.");
#if DEBUG
				Console.ReadKey();
#endif
				return 1;
			}

			try {
				Instance = new Program();
			} catch (Exception e) {
				Logger.Critical("Program", "Application has crashed.", e);
				// At this point it can not be assumed that literally any part of the program is functional, so there's no reporting this crash to Discord or Notification endpoints.
				// At one point someone will notice that the bot is offline and restart it manually.
				// Or if the crash occurred during startup it's likely that the deploy system saw the crash and is doing something about it.
#if DEBUG
				Console.ReadKey();
#endif
				return 2;
			}
			return 0;
		}

		private Program() {
			Logger.Info("Main", "Starting program");

			if (!Directory.Exists(DataPath)) {
				Directory.CreateDirectory(DataPath);
			}

			string configFile = Path.Combine(DataPath, "Config", "Config.json");
			var configService = new GlobalConfigService(configFile);

			IServiceCollection serviceCollection = CreateRBServices(configService);

			Components = new ComponentManager();
			Components.SetupComponents(serviceCollection);

			CommandHandler = new CommandExecutionHandler(Components.Services);
			new CommandExecutedHandler(Components.Services);
			new CommandExceptionHandler(Components.Services);

			/* TODO something needs to notify the ready pipe
			 * Old code for notifying pipe:
			// Find an open Ready pipe and report
			NamedPipeClientStream? pipeClient = null;
			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotReady", PipeDirection.Out);
				await pipeClient.ConnectAsync(1);
				using var sw = new StreamWriter(pipeClient);
				pipeClient = null;
				sw.WriteLine("ready");
			} catch (TimeoutException) {
				// Pass
			} finally {
				if (pipeClient != null) {
					pipeClient.Dispose();
				}
			} 
			*/

			WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			Components.ShutdownComponents();
		}

		private IServiceCollection CreateRBServices(GlobalConfigService configService) {
			var notificationService = new NotificationService();

			var resources = new ResourceService();
			resources.RegisterResources("RoosterBot.Resources");

			var cns = new CultureNameService();
			cns.AddLocalizedName("nl-NL", "nl-NL", "nederlands");
			cns.AddLocalizedName("nl-NL", "en-US", "Dutch");
			cns.AddLocalizedName("en-US", "nl-NL", "engels");
			cns.AddLocalizedName("en-US", "en-US", "English");

			var helpService = new HelpService(resources);
			var commands = new RoosterCommandService(resources);

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(new EmoteService())
				.AddSingleton(configService)
				.AddSingleton(notificationService)
				.AddSingleton(commands)
				.AddSingleton(resources)
				.AddSingleton(helpService)
				.AddSingleton(cns);
			return serviceCollection;
		}

		private void WaitForQuitCondition() {
			var cts = new CancellationTokenSource();
			using (var pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In))
			using (var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true)) {
				_ = pipeServer.WaitForConnectionAsync(cts.Token);

				var quitConditions = new Func<bool>[] {
					() => {
						// Ctrl-Q pressed by user
						if (Console.KeyAvailable) {
							ConsoleKeyInfo keyPress = Console.ReadKey(true);
							if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.Q) {
								Logger.Info("Main", "Ctrl-Q pressed");
								return true;
							}
						}
						return false;
					}, () => {
						// Shutdown() called
						if (m_ShutDown) {
							Logger.Info("Main", "Shutdown() or Restart() has been called");
							return true;
						} else {
							return false;
						}
					}, () => {
						// Pipe connection by stop executable
						if (pipeServer.IsConnected) {
							string? input = sr.ReadLine();
							if (input == "stop") {
								Logger.Info("Main", "Stop command received from external process");
								pipeServer.Disconnect();
								return true;
							} else {
								pipeServer.Disconnect();
								_ = pipeServer.WaitForConnectionAsync(cts.Token);
							}
						}
						return false;
					}
				};

				while (!quitConditions.Any(condition => condition())) {
					// This could make the console window seem unresponsive. Is there a better way?
					Thread.Sleep(500);
				}
			}
			cts.Cancel();
			cts.Dispose();
		}

		public void Shutdown() {
			m_ShutDown = true;
		}

		public void Restart() {
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
			Shutdown();
		}
	}
}
