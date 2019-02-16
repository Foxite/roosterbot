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
using RoosterBot.Services;

namespace RoosterBot {
	public class Program {
		public static Program Instance { get; private set; }

		private ProgramState m_State;

		private bool m_StopFlagSet = false;

		private DiscordSocketClient m_Client;
		private EditedCommandService m_Comands;
		private ConfigService m_ConfigService;
		private IServiceProvider m_Services;

		public event EventHandler ProgramStopping;

		private static void Main(string[] args) {
			Instance = new Program();
			Instance.MainAsync().GetAwaiter().GetResult();
			Console.WriteLine("Async main ended at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
		}

		private async Task MainAsync() {
			Logger.Log(LogSeverity.Info, "Main", "Starting bot");
			m_State = ProgramState.BeforeStart;
			
			#region Load config
			string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoosterBot");
			if (!Directory.Exists(configPath)) {
				Directory.CreateDirectory(configPath);
				Logger.Log(LogSeverity.Critical, "Main", "Config folder did not exist. Please add a Config.json file to the newly created RoosterBot folder in %appdata%.");
				Console.ReadKey();
				return;
			}

			string configFile = Path.Combine(configPath, "Config.json");
			if (!File.Exists(configFile)) {
				Logger.Log(LogSeverity.Critical, "Main", "Config.json file did not exist. Please add a Config.json file to the RoosterBot folder in %appdata%.");
				return;
			}
			string authToken;
			try {
				m_ConfigService = new ConfigService(Path.Combine(configPath, "Config.json"), out authToken);
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reading Config.json file.", ex);
				return;
			}
			#endregion Load config

			#region Start components
			Logger.Log(LogSeverity.Info, "Main", "Preparing to load components");
			// Client is needed by CommandService. Don't start it just yet.
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.MessageReceived += HandleNewCommand;
			m_Client.Ready += async () => {
				m_State = ProgramState.BotRunning;
				await m_Client.SetGameAsync(m_ConfigService.GameString);
				await m_ConfigService.SetLogChannelAsync(m_Client, configPath);

				m_Client.Disconnected += (e) => {
					m_State = ProgramState.BotStopped;
					Task task = Task.Run(() => { // Store task in variable. do not await. just suppress the warning.
						System.Threading.Thread.Sleep(10000);
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
			};

			m_Comands = new EditedCommandService(m_Client, HandleCommand);
			m_Comands.Log += Logger.LogSync;
			await m_Comands.AddModulesAsync(Assembly.GetEntryAssembly());

			IServiceCollection serviceCollection = new ServiceCollection()
				.AddSingleton(m_ConfigService)
				.AddSingleton(m_Comands)
				.AddSingleton(m_Client)
				.AddSingleton(new SNSService(m_ConfigService));

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
			Type[] components = (from domainAssembly in assemblies
							from assemblyType in domainAssembly.GetExportedTypes()
							where assemblyType.IsSubclassOf(typeof(ComponentBase))
							select assemblyType).ToArray();

			// Create instances of these classes and call Initialize()
			foreach (Type type in components) {
				Logger.Log(LogSeverity.Info, "Main", "Loading component " + type.Name);
				ComponentBase component = Activator.CreateInstance(type) as ComponentBase;
				try {
					component.Initialize(ref serviceCollection, m_Comands, Path.Combine(configPath, type.Namespace));
				} catch (Exception ex) {
					Logger.Log(LogSeverity.Critical, "Main", "Component " + type.Name + " threw an exception during initialization.", ex);
					return;
				}
			}
			// And we're done.
			m_Services = serviceCollection.BuildServiceProvider();
			#endregion Start components
			
			#region Start client
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
			// Don't process the command if it was a System Message
			SocketUserMessage message;
			if ((message = command as SocketUserMessage) != null)
				return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (!(message.HasStringPrefix(m_ConfigService.CommandPrefix, ref argPos) || message.HasMentionPrefix(m_Client.CurrentUser, ref argPos))) {
				return;
			}

			if (message.Content.Length == m_ConfigService.CommandPrefix.Length) {
				// Message looks like a command but it does not actually have a command
				return;
			}

			// Create a Command Context
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);
			// Execute the command. (result does not indicate a return value, 
			// rather an object stating if the command executed successfully)
			IResult result = await m_Comands.ExecuteAsync(context, argPos, m_Services);

			await HandleError(result, message, initialResponse);
		}

		public async Task ExecuteSpecificCommand(IUserMessage initialResponse, string specificInput, IUserMessage message) {
			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (!(message.HasStringPrefix(m_Services.GetService<ConfigService>().CommandPrefix, ref argPos) || message.HasMentionPrefix(m_Client.CurrentUser, ref argPos)))
				return;

			// Create a Command Context
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);
			// Execute the command. (result does not indicate a return value, 
			// rather an object stating if the command executed successfully)
			EditedCommandService commandService = m_Services.GetService<EditedCommandService>();
			IResult result = await commandService.ExecuteAsync(context, specificInput, m_Services);

			await HandleError(result, message, initialResponse);
		}

		private async Task HandleError(IResult result, IUserMessage command, IUserMessage initialResponse = null) {
			if (!result.IsSuccess) {
				string response = null;
				bool bad = false;
				string badReport = $"\"{command.Content}\": ";

				if (result.Error.HasValue) {
					switch (result.Error.Value) {
					case CommandError.UnknownCommand:
						response = "Die command ken ik niet.";
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

				if (initialResponse == null) {
					this.m_Comands.AddResponse(command, await command.Channel.SendMessageAsync(response));
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
