using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
		public static readonly Version Version = new(3, 3, 0);

		/// <summary>
		/// The instance of the Program class.
		/// </summary>
		public static Program Instance { get; private set; } = null!;

		/// <summary>
		/// The instance of the <see cref="ComponentManager"/> class.
		/// </summary>
		public ComponentManager Components { get; }

		/// <summary>
		/// The instance of the <see cref="RoosterBot.CommandHandler"/> class.
		/// </summary>
		public CommandHandler CommandHandler { get; }

		private readonly CancellationTokenSource m_ShutDown = new();
		private static readonly IHost ConsoleHost = new HostBuilder().UseConsoleLifetime().Build();

		private static int Main(string[] args) {
			if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0])) {
				DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RoosterBot");
			} else if (Path.IsPathRooted(args[0])) {
				DataPath = args[0];
			} else {
				DataPath = Path.GetFullPath(args[0], Environment.CurrentDirectory);
			}
			
			Mutex? mutex = null;
			try {
				mutex = new Mutex(false, $"RoosterBot-{DataPath.Replace('/', '_')}"); // Works on linux, idk if it works on windows
				if (mutex.WaitOne(1)) {
					ConsoleHost.Start();

					new Program();

					return 0;
				} else {
					// This cannot be logged, because we can't access the log file
					Console.WriteLine("It appears another RoosterBot instance is already running inside the specified data folder.");

					if (!Console.IsInputRedirected) {
						Console.ReadKey();
					}

					return 1;
				}
			} catch (Exception e) {
				Logger.Critical(Logger.Tags.RoosterBot, "Application has crashed.", e);
#if false && DEBUG
				if (!Console.IsInputRedirected) {
					Console.ReadKey();
				}
#endif
				return 2;
			} finally {
				ConsoleHost.Dispose();
				mutex?.Dispose();
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
			
			new CommandExecutedHandler(serviceProvider);
			new CommandExceptionHandler(serviceProvider);
			CommandHandler = new CommandHandler(serviceProvider);
			
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
				.AddSingleton(_ => {
					var ret = new HttpClient();
					ret.DefaultRequestHeaders.Add("User-Agent", $"RoosterBot/{Version}");
					return ret;
				})
				.AddSingleton(resources);
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
				pipeClient?.Dispose();
			}
		}

		private void WaitForQuitCondition() {
			CancellationToken token = m_ShutDown.Token;

			_ = Task.Run(async () => {
				await using var pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In);
				using var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true);

				try {
					await pipeServer.WaitForConnectionAsync(token);
					// Pipe connection by stop executable
					if (pipeServer.IsConnected) {
						Logger.Info(Logger.Tags.RoosterBot, "Stop command received from external process");
						pipeServer.Disconnect();
						m_ShutDown.Cancel();
					}
				} catch (TaskCanceledException) { }
			});

			_ = Task.Run(async () => {
				try {
					await ConsoleHost.WaitForShutdownAsync(token);
					
					// If we get here, then the wait did not get cancelled as we would see the exception here.
					Logger.Info(Logger.Tags.RoosterBot, "SIGTERM received");
					await ConsoleHost.StopAsync();
					m_ShutDown.Cancel();
				} catch (TaskCanceledException) { }
			});

			m_ShutDown.Token.WaitHandle.WaitOne();

			m_ShutDown.Dispose();
		}

		/// <summary>
		/// Signal the program to stop and exit.
		/// </summary>
		public void Shutdown() {
			Logger.Info(Logger.Tags.RoosterBot, "Shutdown() has been called");
			m_ShutDown.Cancel();
		}

		/// <summary>
		/// Signal the program to stop and exit, and then start again.
		/// </summary>
		public void Restart() {
			Logger.Info(Logger.Tags.RoosterBot, "Restart() has been called");
			m_ShutDown.Cancel();
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
		}
	}
}
