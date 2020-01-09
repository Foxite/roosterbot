// TODO 2.3 (refactor) The program currently throws a ton of boneheaded exceptions: https://blogs.msdn.microsoft.com/ericlippert/2008/09/10/vexing-exceptions/
// We really should avoid that wherever possible. Instead of trying to index an array and letting the caller deal with a mysterious "IndexOutOfRangeException",
//  we should catch that before doing anything else and throw an ArgumentException with an actual explanation.

// TODO 2.3 (refactor) Add docstrings to all public things. There's a way to have VS raise a warning for missing docstrings; Qmmands has this enabled, so go steal it from them.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";
#nullable disable
		// These are set during the static Main and MainAsync, but the compiler can't use these methods to determine if they are always set.
		// I would solve this by moving all MainAsync code into the constructor, but:
		// - Constructors can't be async so we can't await anything, which is definitely necessary
		// - You would have to remove all single-use functions like SetupClient, creating a huge constructor which is basically a regression to the pre-2.0 codebase.
		// So I just disable nullable here, because I know it's fine.
		public static Program Instance { get; private set; }

		public ComponentManager Components { get; private set; }

		// TODO find better way of letting components use the standard execution handler
		public CommandExecutionHandler ExecuteHandler { get; private set; }
#nullable restore
		public DateTime StartTime { get; } = DateTime.Now;

		private bool m_StopFlagSet;

		private Program() { }

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
				Instance.MainAsync().GetAwaiter().GetResult();
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

		private async Task MainAsync() {
			Logger.Info("Main", "Starting program");

			string configFile = Path.Combine(DataPath, "Config", "Config.json");
			var configService = new ConfigService(configFile);

			IServiceCollection serviceCollection = CreateRBServices(configService);

			Components = new ComponentManager();
			await Components.SetupComponents(serviceCollection);

			ExecuteHandler = new CommandExecutionHandler(Components.Services);
			new CommandExecutedHandler(Components.Services);
			new CommandExceptionHandler(Components.Services);

			await WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			await Components.ShutdownComponentsAsync();
		}

		private IServiceCollection CreateRBServices(ConfigService configService) {
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

			// Create handlers
			// I don't know what to do with this.
			// We construct a class that fully takes care of itself, does everything it needs to in its constructor (ie subscribing events)
			//  and has no other methods that need to be called, at all.
			// We have a few options with it:
			// - Call the constructor without assigning it to a variable (seems bad form)
			// - Assigning it to a variable without ever using the variable (emits compiler warning)
			// - Adding the object to the ServiceCollection (never used, and nothing you could possibly do with it)
			// I don't know what is the least bad of these options.
			// Though it's really just a style problem, as it does not really affect anything, and the object is never garbage colleted because it creates event handlers
			//  that use the object's fields.

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(configService)
				.AddSingleton(notificationService)
				.AddSingleton(commands)
				.AddSingleton(resources)
				.AddSingleton(helpService)
				.AddSingleton(cns);
			return serviceCollection;
		}

		private async Task WaitForQuitCondition() {
			bool keepRunning = true;

			var cts = new CancellationTokenSource();
			using (var pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In))
			using (var sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true)) {
				_ = pipeServer.WaitForConnectionAsync(cts.Token);

				do {
					keepRunning = true;
					await Task.WhenAny(new Task[] {
						Task.Delay(500).ContinueWith((t) => {
							// Ctrl-Q pressed by user
							if (Console.KeyAvailable) {
								ConsoleKeyInfo keyPress = Console.ReadKey(true);
								if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.Q) {
									keepRunning = false;
									Logger.Info("Main", "Ctrl-Q pressed");
								}
							}
						}),
						Task.Delay(500).ContinueWith((t) => {
							// Stop flag set by RoosterBot or components
							if (m_StopFlagSet) {
								keepRunning = false;
								Logger.Info("Main", "Stop flag set");
							}
						}),
						Task.Delay(500).ContinueWith((t) => {
							// Pipe connection by stop executable
							if (pipeServer.IsConnected) {
								string? input = sr.ReadLine();
								if (input == "stop") {
									Logger.Info("Main", "Stop command received from external process");
									keepRunning = false;
								}
							}
						})
					});
				} while (keepRunning);
			}
			cts.Cancel();
			cts.Dispose();
		}

		public void Shutdown() {
			m_StopFlagSet = true;
		}

		public void Restart() {
			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
			Shutdown();
		}
	}
}
