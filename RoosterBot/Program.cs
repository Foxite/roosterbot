// TODO (refactor) The program currently throws a ton of boneheaded exceptions: https://blogs.msdn.microsoft.com/ericlippert/2008/09/10/vexing-exceptions/
// We really should avoid that wherever possible. Instead of trying to index an array and letting the caller deal with a mysterious "IndexOutOfRangeException",
//  we should catch that before doing anything else and throw an ArgumentException with an actual explanation.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	/// <summary>
	/// The singleton Program class.
	/// </summary>
	public sealed class Program {
		/// <summary>
		/// The version of RoosterBot.
		/// </summary>
		public static readonly Version Version = new Version(3, 0, 0);

		/// <summary>
		/// The directory where program data is stored.
		/// </summary>
		public const string DataPath = @"C:\ProgramData\RoosterBot";

#nullable disable
		/// <summary>
		/// The instance of the Program class.
		/// </summary>
		public static Program Instance { get; private set; }
#nullable restore

		/// <summary>
		/// The instance of the <see cref="ComponentManager"/> class.
		/// </summary>
		public ComponentManager Components { get; private set; }

		/// <summary>
		/// The instance of the <see cref="CommandHandler"/> class.
		/// </summary>
		public CommandHandler CommandHandler { get; private set; }

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
				new Program();
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
			Instance = this;

			if (!Directory.Exists(DataPath)) {
				Directory.CreateDirectory(DataPath);
			}

			string configFolder = Path.Combine(DataPath, "Config");
			if (!Directory.Exists(configFolder)) {
				Directory.CreateDirectory(configFolder);
			}

			Logger.Info("Main", "Starting program");

			IServiceCollection serviceCollection = CreateRBServices();

			Components = new ComponentManager();
			Components.SetupComponents(serviceCollection);

			CommandHandler = new CommandHandler(Components.Services);
			new CommandExecutedHandler(Components.Services);
			new CommandExceptionHandler(Components.Services);
			NotifyAppStart();

			WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			Components.ShutdownComponents();
		}

		private IServiceCollection CreateRBServices() {
			var resources = new ResourceService();
			resources.RegisterResources("RoosterBot.Resources");

			var cns = new CultureNameService();
			// TODO (feature) This should be obtained from resource files
			// Allow any locale to define as many translations as it wants, and just use the first one that is found.
			cns.AddLocalizedName("nl-NL", "nl-NL", "nederlands");
			cns.AddLocalizedName("nl-NL", "en-US", "Dutch");
			cns.AddLocalizedName("en-US", "nl-NL", "engels");
			cns.AddLocalizedName("en-US", "en-US", "English");

			return new ServiceCollection()
				.AddSingleton(new RoosterCommandService(resources))
				.AddSingleton(new NotificationService())
				.AddSingleton(new EmoteService())
				.AddSingleton(resources)
				.AddSingleton(cns);
		}

		private static void NotifyAppStart() {
			// Find an open Ready pipe and report
			NamedPipeClientStream? pipeClient = null;
			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotReady", PipeDirection.Out);
				pipeClient.Connect(1);
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

		/// <summary>
		/// Signal the program to stop and exit.
		/// </summary>
		public void Shutdown() {
			m_ShutDown = true;
		}

		/// <summary>
		/// Signal the program to stop and exit, and then start again.
		/// </summary>
		public void Restart() {
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
			Shutdown();
		}
	}
}
