using System;
using System.Collections.Generic;
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
	// TODO make this less of a god class
	public class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";
		public static Program Instance { get; private set; }

		private ProgramState m_State;
		private bool m_StopFlagSet = false;
		private bool m_VersionNotReported = true;
		private DiscordSocketClient m_Client;
		private EditedCommandService m_Commands;
		private ConfigService m_ConfigService;
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
				Logger.Log(LogSeverity.Critical, "Program", "Application has crashed.", e);
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
			Logger.Log(LogSeverity.Info, "Main", "Starting program");
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
					Logger.Log(LogSeverity.Warning, "Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
				}
			};
			await WaitForQuitCondition();

			Logger.Info("Main", "Stopping program");

			await m_Client.StopAsync();
			await m_Client.LogoutAsync();

			m_State = ProgramState.BotStopped;

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
			m_Client.MessageReceived += (socketMessage) => HandleNewCommand(socketMessage);
		}

		private IServiceCollection CreateRBServices() {
			m_Commands = new EditedCommandService(m_Client);
			m_Commands.Log += Logger.LogSync;
			m_Commands.CommandEdited += HandleEditedCommand;

			HelpService helpService = new HelpService();

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_Commands)
				.AddSingleton(m_Client)
				.AddSingleton(helpService)
				.AddSingleton(new SNSService(m_ConfigService));
			return serviceCollection;
		}

		private Task OnClientConnected() {
			m_State = ProgramState.BotRunning;
			return Task.CompletedTask;
		}

		private Task OnClientDisconnected(Exception ex) {
			m_State = ProgramState.BotStopped;
			Task task = Task.Run(() => { // Store task in variable. do not await. just suppress the warning.
				Thread.Sleep(20000);
				if (m_State != ProgramState.BotRunning) {
					string report = $"RoosterBot has been disconnected for more than twenty seconds. ";
					if (ex == null) {
						report += "No exception is attached.";
					} else {
						report += $"The following exception is attached: \"{ex.Message}\", stacktrace: {ex.StackTrace}";
					}
					report += "\n\nThe bot will attempt to restart in 20 seconds.";
					m_SNSService.SendCriticalErrorNotification(report);

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
			Logger.Log(LogSeverity.Info, "Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

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

		// This function is called by the client.
		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (socketMessage.Source == MessageSource.User && socketMessage is IUserMessage userMessage) {
				await HandleEditedCommand(null, userMessage);
			}
		}

		// This function is called by CommandEditService and the above function.
		// TODO this function should actually only handle edited commands, not all of them
		private async Task HandleEditedCommand(IUserMessage ourResponse, IUserMessage command) {
			int argPos = 0;
			if (!command.HasStringPrefix(m_ConfigService.CommandPrefix, ref argPos)) {
				

				return;
			}

			if (command.Content.Length == m_ConfigService.CommandPrefix.Length) {
				// Message looks like a command but it does not actually have a command
				return;
			}

			EditedCommandContext context = new EditedCommandContext(m_Client, command, ourResponse);

			IResult result = await m_Commands.ExecuteAsync(context, argPos, m_Components.Services);

			await HandleError(context, result);
		}

		public async Task ExecuteSpecificCommand(IUserMessage initialResponse, string specificInput, IUserMessage message) {
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);

			IResult result = await m_Commands.ExecuteAsync(context, specificInput, m_Components.Services);

			await HandleError(context, result);
		}

		// TODO: Use this from CommandService.CommandExecuted instead
		// I have discovered that by doing it that way, you will still get the actual result if the command is RunMode.Async
		// https://discord.foxbot.me/stable/guides/commands/post-execution.html#runtimeresult
		private async Task HandleError(ICommandContext context, IResult result) {
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
							badReport += "Unsuccessful";
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
					Logger.Log(LogSeverity.Error, "Program", "Error occurred while parsing command " + badReport);
					Logger.Log(LogSeverity.Error, "Program", result.ErrorReason);
					if (m_ConfigService.BotOwner != null) {
						await m_ConfigService.BotOwner.SendMessageAsync(badReport);
					}
					await m_SNSService.SendCriticalErrorNotificationAsync(badReport);
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
