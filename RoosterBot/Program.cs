using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RoosterBot.Services;
using System.Linq;

namespace RoosterBot {
	public class Program {
		public static Program Instance { get; private set; }

		internal ProgramState State { get; set; }

		private bool m_StopFlagSet = false;

		private DiscordSocketClient m_Client;
		private EditedCommandService m_Comands;
		private ConfigService m_ConfigService;
		private IServiceProvider m_Services;

		private static void Main(string[] args) {
			Instance = new Program();
			Instance.MainAsync().GetAwaiter().GetResult();
			Console.WriteLine("Press any key to quit.");
			Console.ReadKey(true);
		}

		private async Task MainAsync() {
			Logger.Log(LogSeverity.Info, "Main", "Starting bot");
			State = ProgramState.BeforeStart;

			List<Task> concurrentLoading = new List<Task>();

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
			foreach (string file in toLoad) {
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
				var a = File.Exists(path);
				var b = Path.GetExtension(path).ToLower() == ".dll";
				if (a && b) {
					AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path));
				} else {
					Logger.Log(LogSeverity.Error, "Main", "Component " + file + " does not exist or it is not a DLL file");
				}
			}

			// Look for children of ComponentBase in all assemblies (takes a while)
			Type[] components = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
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

			#region Finish initialization tasks
			try {
				Task.WaitAll(concurrentLoading.ToArray());
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "One or more errors occurred in the background initialization tasks.", ex);
				return;
			}
			#endregion

			#region Start client
			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();
			#endregion Start client

			#region Quit code
			m_Client.Ready += async () => {
				State = ProgramState.BotRunning;
				await m_Client.SetGameAsync(m_ConfigService.GameString);
			};

			Console.CancelKeyPress += (o, e) => {
				if (State != ProgramState.BotStopped) {
					e.Cancel = true;
					Logger.Log(LogSeverity.Warning, "Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
				}
			};

			ConsoleKeyInfo keyPress;
			bool keepRunning;
			do {
				keepRunning = true;
				Task.WaitAny(new Task[] {
					Task.Delay(500).ContinueWith((t) => {
						if (Console.KeyAvailable) {
							keyPress = Console.ReadKey(true);
							if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.Q) {
								keepRunning = false;
								Logger.Log(LogSeverity.Info, "Main", "Ctrl-Q pressed");
							}
						}
					}),
					Task.Delay(500).ContinueWith((t) => {
						if (m_StopFlagSet) {
							keepRunning = false;
							Logger.Log(LogSeverity.Info, "Main", "Stop flag set");
						}
					})
				});

			} while (State == ProgramState.BeforeStart || keepRunning); // Program cannot be stopped before initialization is complete

			Logger.Log(LogSeverity.Info, "Main", "Stopping bot");
			await m_Client.StopAsync();
			await m_Client.LogoutAsync();
			State = ProgramState.BotStopped;
			#endregion Quit code
		}

		// This function is given to the CommandService.
		private async Task HandleNewCommand(SocketMessage command) {
			await HandleCommand(null, command);
		}

		// This function is called by CommandEditService and the above function.
		public async Task HandleCommand(IUserMessage initialResponse, SocketMessage command) {
			// Don't process the command if it was a System Message
			if (!(command is SocketUserMessage message))
				return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (!(message.HasStringPrefix(m_ConfigService.CommandPrefix, ref argPos) || message.HasMentionPrefix(m_Client.CurrentUser, ref argPos)))
				return;
			// Create a Command Context
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse);
			// Execute the command. (result does not indicate a return value, 
			// rather an object stating if the command executed successfully)
			IResult result = await m_Comands.ExecuteAsync(context, argPos, m_Services);
			if (!result.IsSuccess) {
				if (initialResponse == null) {
					IUserMessage response = await context.Channel.SendMessageAsync(result.ErrorReason);
					m_Comands.AddResponse(context.Message, response);
				} else {
					await initialResponse.ModifyAsync((msgProps) => { msgProps.Content = result.ErrorReason; });
				}
			}
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
			if (!result.IsSuccess) {
				if (initialResponse == null) {
					IUserMessage response = await context.Channel.SendMessageAsync(result.ErrorReason);
					commandService.AddResponse(context.Message, response);
				} else {
					await initialResponse.ModifyAsync((msgProps) => { msgProps.Content = result.ErrorReason; });
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
