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
		/// The directory where program data is stored.
		/// </summary>
		public const string DataPath = @"C:\ProgramData\RoosterBot";

		/// <summary>
		/// The version of RoosterBot.
		/// </summary>
		public static readonly Version Version = new Version(3, 0, 0);

		/// <summary>
		/// The instance of the Program class.
		/// </summary>
		public static Program Instance { get; private set; } = null!;

		/// <summary>
		/// The instance of the <see cref="ComponentManager"/> class.
		/// </summary>
		public ComponentManager Components { get; private set; }

		/// <summary>
		/// The instance of the <see cref="RoosterBot.CommandHandler"/> class.
		/// </summary>
		public CommandHandler CommandHandler { get; private set; }

		private bool m_ShutDown;
		
		private static int Main(string[] args) {
			if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
				Console.WriteLine("There is already a process named RoosterBot running. There cannot be more than one instance of the bot.");
				Console.ReadKey();
				return 1;
			}

			try {
				new Program();
				return 0;
			} catch (Exception e) {
				Logger.Critical("Program", "Application has crashed.", e);
#if DEBUG
				Console.ReadKey();
#endif
				return 2;
			}
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

			Logger.AddEndpoint(new FileLogEndpoint());
			Logger.AddEndpoint(new ConsoleLogEndpoint());

			Logger.Info("Main", "Starting program");

			Components = new ComponentManager();
			IServiceCollection serviceCollection = CreateRBServices();
			IServiceProvider serviceProvider = Components.SetupComponents(serviceCollection);
			CommandHandler = CreateHandlers(serviceProvider);
			NotifyAppStart();

			WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			Components.ShutdownComponents();
		}

		private IServiceCollection CreateRBServices() {
			var resources = new ResourceService();
			resources.RegisterResources("RoosterBot.Resources");

			return new ServiceCollection()
				.AddSingleton(new RoosterCommandService(resources))
				.AddSingleton(new HelpService(resources))
				.AddSingleton(new NotificationService())
				.AddSingleton(new EmoteService())
				.AddSingleton(new CultureNameService(resources))
				.AddSingleton(new Random())
				.AddSingleton(resources);
		}

		private CommandHandler CreateHandlers(IServiceProvider services) {
			new CommandExecutedHandler(services);
			new CommandExceptionHandler(services);
			return new CommandHandler(services);
		}

		private void NotifyAppStart() {
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
			Console.CancelKeyPress += (o, e) => {
				e.Cancel = true;
				m_ShutDown = true;
				Logger.Info("Main", "Ctrl-C pressed");
			};

			var cts = new CancellationTokenSource();
			using (var pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In))
			using (var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true)) {
				_ = pipeServer.WaitForConnectionAsync(cts.Token);

				var quitConditions = new Func<bool>[] {
					() => m_ShutDown,
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
			Logger.Info("Main", "Shutdown() has been called");
			m_ShutDown = true;
		}

		/// <summary>
		/// Signal the program to stop and exit, and then start again.
		/// </summary>
		public void Restart() {
			Logger.Info("Main", "Restart() has been called");
			m_ShutDown = true;
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
		}
	}
}
