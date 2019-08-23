using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";
		public static Program Instance { get; private set; }

		private ProgramState m_State; // TODO can we use m_Client.ConnectionState instead?
		private bool m_StopFlagSet;
		private bool m_VersionNotReported = true;
		private DiscordSocketClient m_Client;
		private ConfigService m_ConfigService;
		private SNSService m_SNSService;

		public ComponentManager Components { get; private set; }
		public CommandHandler CommandHandler { get; set; }
		private static int Main(string[] args) {
			string indicatorPath = Path.Combine(DataPath, "running");

			if (File.Exists(indicatorPath)) {
				Console.WriteLine("Bot already appears to be running. Delete the \"running\" file in the ProgramData folder to override this.");
				return 1;
			} else {
				File.Create(indicatorPath).Dispose();
			}

			try {
				Instance = new Program();
				Instance.MainAsync().GetAwaiter().GetResult();
			} catch (Exception e) {
				Logger.Critical("Program", "Application has crashed.", e);
#if DEBUG
				Console.ReadKey();
#endif
				return 2;
			} finally {
				File.Delete(indicatorPath);
			}
			return 0;
		}

		private async Task MainAsync() {
			Logger.Info("Main", "Starting program");
			m_State = ProgramState.BeforeStart;

			string configFile = Path.Combine(DataPath, "Config", "Config.json");
			m_ConfigService = new ConfigService(configFile, out string authToken);

			SetupClient();

			IServiceCollection serviceCollection = CreateRBServices();

			CommandHandler = new CommandHandler(serviceCollection, m_ConfigService, m_Client);

			Components = await ComponentManager.CreateAsync(serviceCollection);

			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();

			Console.CancelKeyPress += (o, e) => {
				e.Cancel = true;
				Logger.Warning("Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
			};
			await WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			await m_Client.StopAsync();
			await m_Client.LogoutAsync();

			await Components.ShutdownComponentsAsync();
			m_Client.Dispose();
		}

		private async Task WaitForQuitCondition() {
			ConsoleKeyInfo keyPress;
			bool keepRunning = true;

			CancellationTokenSource cts = new CancellationTokenSource();
			using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In)) {
				_ = pipeServer.WaitForConnectionAsync(cts.Token);

				do {
					keepRunning = true;
					await Task.WhenAny(new Task[] {
						Task.Delay(500).ContinueWith((t) => {
							// Ctrl-Q pressed by user
							if (Console.KeyAvailable) {
								keyPress = Console.ReadKey(true);
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
								using (StreamReader sr = new StreamReader(pipeServer)) {
									string input = sr.ReadLine();
									if (input == "stop") {
										Logger.Info("Main", "Stop command received from external process");
										keepRunning = false;
									}
								}
							}
						})
					});

				} while (m_State == ProgramState.BeforeStart || keepRunning); // Program cannot be stopped before initialization is complete
			}
			cts.Cancel();
			cts.Dispose();
		}

		private void SetupClient() {
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.Ready += OnClientReady;
			m_Client.Connected += OnClientConnected;
			m_Client.Disconnected += OnClientDisconnected;
		}

		private IServiceCollection CreateRBServices() {
			HelpService helpService = new HelpService();
			m_SNSService = new SNSService(m_ConfigService);

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_SNSService)
				.AddSingleton(helpService)
				.AddSingleton(m_Client);
			return serviceCollection;
		}

		private Task OnClientConnected() {
			m_State = ProgramState.BotRunning;
			return Task.CompletedTask;
		}

		private Task OnClientDisconnected(Exception ex) {
			m_State = ProgramState.BotStopped;
			Task task = Task.Run(async () => { // Store task in variable. do not await. just suppress the warning.
				await Task.Delay(20000);
				if (m_State != ProgramState.BotRunning) {
					string report = $"RoosterBot has been disconnected for more than twenty seconds. ";
					if (ex == null) {
						report += "No exception is attached.";
					} else {
						report += $"The following exception is attached: {ex.ToStringDemystified()}";
					}
					report += "\n\nThe bot will attempt to restart in 20 seconds.";
					await m_SNSService.SendCriticalErrorNotificationAsync(report);

					Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
					Shutdown();
				}
			});

			return Task.CompletedTask;
		}

		private async Task OnClientReady() {
			m_State = ProgramState.BotRunning;
			await m_ConfigService.LoadDiscordInfo(m_Client);
			await m_Client.SetGameAsync(m_ConfigService.GameString, type: m_ConfigService.ActivityType);
			Logger.Info("Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_ConfigService.ReportStartupVersionToOwner) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await m_ConfigService.BotOwner.GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Constants.VersionString}\n";
				startReport += "Components:\n";
				foreach (ComponentBase component in Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.VersionString}\n";
				}

				await ownerDM.SendMessageAsync(startReport);
			}
		}

		/// <summary>
		/// Shuts down gracefully.
		/// </summary>
		public void Shutdown() {
			m_StopFlagSet = true;
		}
	}

	public enum ProgramState {
		BeforeStart, BotRunning, BotStopped
	}
}
