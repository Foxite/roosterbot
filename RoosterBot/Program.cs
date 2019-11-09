using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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

		private DiscordSocketClient m_Client;
		private ConfigService m_ConfigService;
		private NotificationService m_NotificationService;
#nullable restore
		private bool m_BeforeStart;
		private bool m_StopFlagSet;
		private bool m_VersionNotReported = true;

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
			m_BeforeStart = true;
			
			string configFile = Path.Combine(DataPath, "Config", "Config.json");
			m_ConfigService = new ConfigService(configFile, out string authToken);

			SetupClient();

			IServiceCollection serviceCollection = CreateRBServices();

			Components = new ComponentManager();
			await Components.SetupComponents(serviceCollection);

			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();

			await WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			await m_Client.StopAsync();
			await m_Client.LogoutAsync();

			Components.ShutdownComponents();
			m_Client.Dispose();
		}

		private void SetupClient() {
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.Ready += OnClientReady;
		}

		private IServiceCollection CreateRBServices() {
			m_NotificationService = new NotificationService();
			CommandResponseService crs = new CommandResponseService(m_ConfigService);

			ResourceService resources = new ResourceService();
			resources.RegisterResources("RoosterBot.Resources");

			CultureNameService cns = new CultureNameService();
			cns.AddLocalizedName("nl-NL", "nl-NL", "nederlands");
			cns.AddLocalizedName("nl-NL", "en-US", "Dutch");
			cns.AddLocalizedName("en-US", "nl-NL", "engels");
			cns.AddLocalizedName("en-US", "en-US", "English");

			HelpService helpService = new HelpService(resources);
			RoosterCommandService commands = new RoosterCommandService(resources);
			commands.Log += Logger.LogSync;

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
			new RestartHandler(m_Client, m_NotificationService, 5);
			new DeadlockHandler(m_Client, m_NotificationService, 60000);

			// TODO do this during AddModules, it needs GuildConfigService but it's not available at this point anymore
			//new NewCommandHandler(m_Client, commands, m_ConfigService, gcs);
			//new EditedCommandHandler(m_Client, commands, m_ConfigService, crs, gcs);
			//new PostCommandHandler(commands, m_ConfigService, gcs, resources, crs);
			//new DeletedCommandHandler(m_Client, crs);

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_NotificationService)
				.AddSingleton(commands)
				.AddSingleton(resources)
				.AddSingleton(helpService)
				.AddSingleton(crs)
				.AddSingleton(m_Client);
			return serviceCollection;
		}

		private async Task OnClientReady() {
			try {
				await m_ConfigService.LoadDiscordInfo(m_Client);
				await m_Client.SetGameAsync(m_ConfigService.GameString, type: m_ConfigService.ActivityType);
				Logger.Info("Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

				if (m_VersionNotReported && m_ConfigService.ReportStartupVersionToOwner) {
					m_VersionNotReported = false;
					IDMChannel ownerDM = await m_ConfigService.BotOwner.GetOrCreateDMChannelAsync();
					string startReport = $"RoosterBot version: {Constants.VersionString}\n";
					startReport += "Components:\n";
					foreach (ComponentBase component in Components.GetComponents()) {
						startReport += $"- {component.Name}: {component.ComponentVersion.ToString()}\n";
					}

					await ownerDM.SendMessageAsync(startReport);
				}

				// Find an open Ready pipe and report
				NamedPipeClientStream? pipeClient = null;
				try {
					pipeClient = new NamedPipeClientStream(".", "roosterbotReady", PipeDirection.Out);
					await pipeClient.ConnectAsync(1);
					using StreamWriter sw = new StreamWriter(pipeClient);
					pipeClient = null;
					sw.WriteLine("ready");
				} catch (TimeoutException) {
					// Pass
				} finally {
					if (pipeClient != null) {
						pipeClient.Dispose();
					}
				}
			} finally {
				// Make sure the program can be stopped gracefully regardless of any exceptions that occur here
				m_BeforeStart = false;
			}
		}

		private async Task WaitForQuitCondition() {
			bool keepRunning = true;

			CancellationTokenSource cts = new CancellationTokenSource();
			using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In))
			using (StreamReader sr = new StreamReader(pipeServer, Encoding.UTF8, true, 512, true)) {
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
				} while (m_BeforeStart || keepRunning); // Program cannot be stopped before initialization is complete
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
