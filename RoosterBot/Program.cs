using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Attributes;
using RoosterBot.Services;

namespace RoosterBot {
	public class Program {
		public const string DataPath = @"C:\ProgramData\RoosterBot";
		public static Program Instance { get; private set; }

		private ProgramState m_State;
		private bool m_StopFlagSet = false;
		private DiscordSocketClient m_Client;
		private EditedCommandService m_Commands;
		private ConfigService m_ConfigService;
		private IServiceProvider m_Services;
		private Dictionary<Type, ComponentBase> m_Components;

		public event EventHandler ProgramStopping;

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
			} catch {
				return 2;
			} finally {
				File.Delete(indicatorPath);
			}
			return 0;
		}

		private async Task MainAsync() {
			Logger.Log(LogSeverity.Info, "Main", "Starting bot");
			m_State = ProgramState.BeforeStart;
			
			#region Load config
			if (!Directory.Exists(DataPath)) {
				Logger.Log(LogSeverity.Critical, "Main", "Data folder did not exist.");
				throw new InvalidOperationException("Data folder did not exist.");
			}

			string configFile = Path.Combine(DataPath, "Config", "Config.json");
			if (!File.Exists(configFile)) {
				Logger.Log(LogSeverity.Critical, "Main", "Config file did not exist.");
				throw new InvalidOperationException("Config file did not exist.");
			}
			string authToken;
			try {
				m_ConfigService = new ConfigService(configFile, out authToken);
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reading Config.json file.", ex);
				throw;
			}
			#endregion Load config

			#region Start client
			Logger.Log(LogSeverity.Info, "Main", "Preparing to load components");
			// Client is needed by CommandService. Don't start it just yet.
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.MessageReceived += HandleNewCommand;
			m_Client.Ready += async () => {
				m_State = ProgramState.BotRunning;
				await m_Client.SetGameAsync(m_ConfigService.GameString, type: ActivityType.Watching);
				await m_ConfigService.SetLogChannelAsync(m_Client, Path.Combine(DataPath, "config"));
				Logger.Log(LogSeverity.Info, "Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

				m_Client.Disconnected += (e) => {
					m_State = ProgramState.BotStopped;
					Task task = Task.Run(() => { // Store task in variable. do not await. just suppress the warning.
						Thread.Sleep(10000);
						if (m_State != ProgramState.BotRunning) {
							string report = $"RoosterBot has been disconnected for more than ten seconds. ";
							if (e == null) {
								report += "No exception is attached.";
							} else {
								report += $"The following exception is attached: \"{e.Message}\", stacktrace: {e.StackTrace}";
							}
							m_Services.GetService<SNSService>().SendCriticalErrorNotification(report);
						}
					});

					return Task.CompletedTask;
				};

				m_Client.Connected += () => {
					m_State = ProgramState.BotRunning;
					return Task.CompletedTask;
				};

#if !DEBUG
				IDMChannel ownerDM = await m_Client.GetUser(m_ConfigService.BotOwnerId).GetOrCreateDMChannelAsync();
				await ownerDM.SendMessageAsync("New version deployed: " + Constants.VersionString);
#endif
			};

			m_Commands = new EditedCommandService(m_Client, HandleCommand);
			m_Commands.Log += Logger.LogSync;

			HelpService helpService = new HelpService();

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_Commands)
				.AddSingleton(m_Client)
				.AddSingleton(helpService)
				.AddSingleton(new SNSService(m_ConfigService));

			#endregion

			#region Start components
			Logger.Log(LogSeverity.Info, "Main", "Loading Components");
			
			// Locate DLL files from a txt file
			string[] toLoad = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "components.txt"));
			List<Assembly> assemblies = new List<Assembly>();
			foreach (string file in toLoad) {
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
				if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".dll") {
					assemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
				} else {
					Logger.Log(LogSeverity.Error, "Main", "Component " + file + " does not exist or it is not a DLL file");
				}
			}

			// Look for children of ComponentBase in the loaded assemblies
			Type[] componentTypes = (from domainAssembly in assemblies
								 from assemblyType in domainAssembly.GetExportedTypes()
								 where assemblyType.IsSubclassOf(typeof(ComponentBase))
								 select assemblyType).ToArray();

			m_Components = new Dictionary<Type, ComponentBase>(componentTypes.Length);
			// Create instances of these classes and call AddServices and then AddModules
			for (int i = 0; i < componentTypes.Length; i++) {
				Type type = componentTypes[i];
				Logger.Log(LogSeverity.Info, "Main", "Adding services from " + type.Name);
				m_Components[type] = Activator.CreateInstance(type) as ComponentBase;
				try {
					m_Components[type].AddServices(ref serviceCollection, Path.Combine(DataPath, "Config", type.Namespace));
				} catch (Exception ex) {
					Logger.Log(LogSeverity.Critical, "Main", "Component " + type.Name + " threw an exception during AddServices.", ex);
					return;
				}
			}

			m_Services = serviceCollection.BuildServiceProvider();
			
			await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly(), m_Services);

			foreach (KeyValuePair<Type, ComponentBase> componentKVP in m_Components) {
				Logger.Log(LogSeverity.Info, "Main", "Adding modules from " + componentKVP.Key.Name);
				try {
					componentKVP.Value.AddModules(m_Services, m_Commands, helpService);
				} catch (Exception ex) {
					Logger.Log(LogSeverity.Critical, "Main", "Component " + componentKVP.Key.Name + " threw an exception during AddModules.", ex);
					return;
				}
			}
			#endregion Start components

			#region Connect to Discord
			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();
			#endregion Start client

			#region Quit code
			Console.CancelKeyPress += (o, e) => {
				if (m_State != ProgramState.BotStopped) {
					e.Cancel = true;
					Logger.Log(LogSeverity.Warning, "Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
				}
			};

			ConsoleKeyInfo keyPress;
			bool keepRunning = true;

			CancellationTokenSource cts = new CancellationTokenSource();
			using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("roosterbotStopPipe", PipeDirection.In)) {
				Task pipeWait = pipeServer.WaitForConnectionAsync(cts.Token);

				do {
					keepRunning = true;
					Task.WaitAny(new Task[] {
						Task.Delay(500).ContinueWith((t) => {
							// Ctrl-Q pressed by user
							if (Console.KeyAvailable) {
								keyPress = Console.ReadKey(true);
								if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.Q) {
									keepRunning = false;
									Logger.Log(LogSeverity.Info, "Main", "Ctrl-Q pressed");
								}
							}
						}),
						Task.Delay(500).ContinueWith((t) => {
							// Stop flag set by RoosterBot or components
							if (m_StopFlagSet) {
								keepRunning = false;
								Logger.Log(LogSeverity.Info, "Main", "Stop flag set");
							}
						}),
						Task.Delay(500).ContinueWith((t) => {
							// Pipe connection by stop executable
							if (pipeServer.IsConnected) {
								using (StreamReader sr = new StreamReader(pipeServer)) {
									string input = sr.ReadLine();
									if (input == "stop") {
										Console.WriteLine("Stop command received by external process");
										keepRunning = false;
									}
								}
								
							}
						})
					});

				} while (m_State == ProgramState.BeforeStart || keepRunning); // Program cannot be stopped before initialization is complete
			}
			cts.Cancel();

			Logger.Log(LogSeverity.Info, "Main", "Stopping bot");
			
			await m_Client.StopAsync();
			await m_Client.LogoutAsync();

			ProgramStopping?.Invoke(this, null);

			m_State = ProgramState.BotStopped;
			#endregion Quit code
		}

		// This function is given to the CommandService.
		private async Task HandleNewCommand(SocketMessage command) {
			await HandleCommand(null, command);
		}

		// This function is called by CommandEditService and the above function.
		public async Task HandleCommand(IUserMessage initialResponse, SocketMessage command) {
			// Don't process the command if it was a System Message or came from a bot
			if (!(command is SocketUserMessage message) || message.Author.IsBot)
				return;
			
			int argPos = 0;
			if (!message.HasStringPrefix(m_ConfigService.CommandPrefix, ref argPos)) {
				return;
			}

			if (message.Content.Length == m_ConfigService.CommandPrefix.Length) {
				// Message looks like a command but it does not actually have a command
				return;
			}
			
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);

			IResult result = await m_Commands.ExecuteAsync(context, argPos, m_Services);

			await HandleError(context, result);
		}

		public async Task ExecuteSpecificCommand(IUserMessage initialResponse, string specificInput, IUserMessage message) {
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);

			IResult result = await m_Commands.ExecuteAsync(context, specificInput, m_Services);

			await HandleError(context, result);
		}
		
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
						response += result.ErrorReason;
						break;
					case CommandError.ParseFailed:
						badReport += "ParseFailed";
						bad = true;
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
					if (m_ConfigService.LogChannel != null) {
						await m_ConfigService.LogChannel.SendMessageAsync(m_Client.GetUser(m_ConfigService.BotOwnerId).Mention + " " + badReport);
					}
					await m_Services.GetService<SNSService>().SendCriticalErrorNotificationAsync(badReport);
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
