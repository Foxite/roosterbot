using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RoosterBot {
	/// <summary>
	/// The singleton Program class.
	/// </summary>
	public sealed class Program {
		/// <summary>
		/// The directory where program data is stored.
		/// </summary>
		public static string DataPath { get; private set; } = "";

		/// <summary>
		/// The version of RoosterBot.
		/// </summary>
		public static readonly Version Version = new Version(3, 3, 0);

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
		private static readonly IHost ConsoleHost = new HostBuilder().UseConsoleLifetime().Build();

		private static int Main(string[] args) {
			if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
				Console.WriteLine("There is already a process named RoosterBot running. There cannot be more than one instance of the bot.");
				Console.ReadKey();
				return 1;
			}

			try {
				DataPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, args[0]);

				ConsoleHost.Start();

				new Program();
				return 0;
			} catch (Exception e) {
				Logger.Critical(Logger.Tags.RoosterBot, "Application has crashed.", e);
#if DEBUG
				if (!Console.IsInputRedirected) {
					Console.ReadKey();
				}
#endif
				return 2;
			} finally {
				ConsoleHost.Dispose();
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

			Logger.Info(Logger.Tags.RoosterBot, "Starting program");

			Components = new ComponentManager();
			IServiceCollection serviceCollection = CreateRBServices();
			IServiceProvider serviceProvider = Components.SetupComponents(serviceCollection);
			CommandHandler = CreateHandlers(serviceProvider);
			NotifyAppStart();

			WaitForQuitCondition();

			Logger.Info(Logger.Tags.RoosterBot, "Stopping program");

			Components.ShutdownComponents();
		}

		private IServiceCollection CreateRBServices() {
			var resources = new ResourceService();
			resources.RegisterResources("RoosterBot.Resources");

			return new ServiceCollection()
				.AddSingleton(new RoosterCommandService(resources))
				.AddSingleton(new NotificationService())
				.AddSingleton(new EmoteService())
				.AddSingleton(new CultureNameService(resources))
				.AddSingleton(new Random())
				.AddSingleton(isp => {
					var ret = new HttpClient();
					ret.DefaultRequestHeaders.Add("User-Agent", $"RoosterBot/{Version}");
					return ret;
				})
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
			var cts = new CancellationTokenSource();
			using (var pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In))
			using (var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true)) {
				CancellationToken token = cts.Token;
				_ = pipeServer.WaitForConnectionAsync(token);

				Task consoleShutdown = ConsoleHost.WaitForShutdownAsync(token)
					.ContinueWith(t => {
						if (!token.IsCancellationRequested) {
							Logger.Info(Logger.Tags.RoosterBot, "SIGTERM received");
						}
						ConsoleHost.StopAsync();
					});

				var quitConditions = new List<Func<bool>>() {
					() => m_ShutDown,
					() => {
						// Pipe connection by stop executable
						if (pipeServer.IsConnected) {
							string? input = sr.ReadLine();
							if (input == "stop") {
								Logger.Info(Logger.Tags.RoosterBot, "Stop command received from external process");
								pipeServer.Disconnect();
								return true;
							} else {
								pipeServer.Disconnect();
								_ = pipeServer.WaitForConnectionAsync(cts.Token);
							}
						}
						return false;
					},
					() => consoleShutdown.IsCompleted
				};

				if (!Console.IsInputRedirected) {
					quitConditions.Add(() => {
						// Ctrl-Q pressed by user
						if (Console.KeyAvailable) {
							ConsoleKeyInfo keyPress = Console.ReadKey(true);
							if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.Q) {
								Logger.Info(Logger.Tags.RoosterBot, "Ctrl-Q pressed");
								return true;
							}
						}
						return false;
					});
				}

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
			Logger.Info(Logger.Tags.RoosterBot, "Shutdown() has been called");
			m_ShutDown = true;
		}

		/// <summary>
		/// Signal the program to stop and exit, and then start again.
		/// </summary>
		public void Restart() {
			Logger.Info(Logger.Tags.RoosterBot, "Restart() has been called");
			m_ShutDown = true;
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
		}
	}
}
