using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";
		public static Program Instance { get; private set; }
		
		private ProgramState m_State; // TODO can we use m_Client.ConnectionState instead?
		private bool m_StopFlagSet = false;
		private bool m_VersionNotReported = true;
		private DiscordSocketClient m_Client;
		private EditedCommandService m_Commands;
		private ConfigService m_ConfigService;
		private CloudWatchReporter m_CloudWatchReporter;
		private SNSService m_SNSService;
		private ComponentManager m_Components;

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

			m_Components = await ComponentManager.CreateAsync(serviceCollection);

			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();

			#region Quit code
			Console.CancelKeyPress += (o, e) => {
				if (m_State != ProgramState.BotStopped) {
					e.Cancel = true;
					Logger.Warning("Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
				}
			};
			await WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			await m_Client.StopAsync();
			await m_Client.LogoutAsync();

			m_State = ProgramState.BotStopped;

			m_CloudWatchReporter.Dispose();

			await m_Components.ShutdownComponentsAsync();
			m_Client.Dispose();

			#endregion Quit code
		}

		private async Task WaitForQuitCondition() {
			ConsoleKeyInfo keyPress;
			bool keepRunning = true;

			CancellationTokenSource cts = new CancellationTokenSource();
			using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In)) {
				Task pipeWait = pipeServer.WaitForConnectionAsync(cts.Token);

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
		}

		private void SetupClient() {
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.Ready += OnClientReady;
			m_Client.Connected += OnClientConnected;
			m_Client.Disconnected += OnClientDisconnected;
			m_Client.MessageReceived += HandleNewCommand;
		}

		private IServiceCollection CreateRBServices() {
			m_Commands = new EditedCommandService(m_Client);
			m_Commands.Log += Logger.LogSync;
			m_Commands.CommandEdited += HandleEditedCommand;
			m_Commands.CommandExecuted += OnCommandExecuted;

			HelpService helpService = new HelpService();
			m_SNSService = new SNSService(m_ConfigService);
			m_CloudWatchReporter = new CloudWatchReporter(m_Client);

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_SNSService)
				.AddSingleton(helpService)
				.AddSingleton(m_Commands)
				.AddSingleton(m_Client);
			return serviceCollection;
		}

		private bool IsMessageCommand(IMessage message, out int argPos) {
			argPos = 0;
			if (message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > m_ConfigService.CommandPrefix.Length &&
				userMessage.HasStringPrefix(m_ConfigService.CommandPrefix, ref argPos)) {
				// First char after prefix
				char firstChar = message.Content.Substring(m_ConfigService.CommandPrefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}

		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (IsMessageCommand(socketMessage, out int argPos)) {
				EditedCommandContext context = new EditedCommandContext(m_Client, socketMessage as IUserMessage, null);

				await m_Commands.ExecuteAsync(context, argPos, m_Components.Services);
			}
		}

		private async Task HandleEditedCommand(IUserMessage ourResponse, IUserMessage command) {
			if (IsMessageCommand(command, out int argPos)) {
				EditedCommandContext context = new EditedCommandContext(m_Client, command, ourResponse);

				await m_Commands.ExecuteAsync(context, argPos, m_Components.Services);
			} else {
				await ourResponse.DeleteAsync();
			}
		}

		/// <summary>
		/// Executes a command according to specified string input, regardless of the actual content of the message.
		/// </summary>
		/// <param name="calltag">Used for debugging. This identifies where this call originated.</param>
		public async Task ExecuteSpecificCommand(IUserMessage initialResponse, string specificInput, IUserMessage message, string calltag) {
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse, calltag);

			Logger.Debug("Main", $"Executing specific input `{specificInput}` with calltag `{calltag}`");

			await m_Commands.ExecuteAsync(context, specificInput, m_Components.Services);
		}

		private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result) {
			if (!result.IsSuccess) {
				string response = null;
				bool bad = false;
				string badReport = $"\"{context.Message}\": ";

				if (result.Error.HasValue) {
					switch (result.Error.Value) {
						case CommandError.UnknownCommand:
							response = "Die command ken ik niet. Gebruik `!help` voor informatie.";
							break;
						case CommandError.BadArgCount:
							response = "Dat zijn te veel of te weinig parameters.";
							break;
						case CommandError.UnmetPrecondition:
							response = result.ErrorReason;
							break;
						case CommandError.ParseFailed:
							response = "Ik begrijp de parameter(s) niet.";
							break;
						case CommandError.ObjectNotFound:
							badReport += "ObjectNotFound";
							bad = true;
							break;
						case CommandError.MultipleMatches:
							badReport += "MultipleMatches";
							bad = true;
							break;
						case CommandError.Exception:
							badReport += "Exception\n";
							badReport += result.ErrorReason;
							bad = true;
							break;
						case CommandError.Unsuccessful:
							badReport += "Unsuccessful\n";
							badReport += result.ErrorReason;
							bad = true;
							break;
						default:
							badReport += "Unknown error: " + result.Error.Value;
							bad = true;
							break;
					}
				} else {
					badReport += "No error reason";
					bad = true;
				}

				if (bad) {
					Logger.Error("Program", "Error occurred while parsing command " + badReport);
					if (m_ConfigService.BotOwner != null) {
						await m_ConfigService.BotOwner.SendMessageAsync(badReport);
					}

					response = "Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.";
				}

				IUserMessage initialResponse = (context as EditedCommandContext)?.OriginalResponse;
				if (initialResponse == null) {
					m_Commands.AddResponse(context.Message, await context.Channel.SendMessageAsync(response));
				} else {
					await initialResponse.ModifyAsync((msgProps) => { msgProps.Content = response; });
				}
			}
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
			await m_ConfigService.LoadDiscordInfo(m_Client, Path.Combine(DataPath, "config"));
			await m_Client.SetGameAsync(m_ConfigService.GameString, type: m_ConfigService.ActivityType);
			Logger.Info("Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_ConfigService.ReportStartupVersionToOwner) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await m_ConfigService.BotOwner.GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Constants.VersionString}\n";
				startReport += "Components:\n";
				foreach (ComponentBase component in m_Components.GetComponents()) {
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
