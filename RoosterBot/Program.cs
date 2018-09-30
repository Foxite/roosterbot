using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Reflection;
using Discord.Net.Providers.WS4Net;
using System.Collections.Generic;
using System.IO;

namespace RoosterBot {
	public class Program {
		public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient m_Client;
		private IServiceProvider m_Services;
		private CommandService m_Commands;

		public async Task MainAsync() {
			Logger.Log(LogSeverity.Info, "Main", "Starting bot");

			#region Load config
			string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoosterBot");
			if (!Directory.Exists(configPath)) {
				Directory.CreateDirectory(configPath);
				Logger.Log(LogSeverity.Critical, "Main", "Config folder did not exist. Please add a config file to the newly created RoosterBot folder in %appdata%.");
				Console.ReadKey();
				return;
			}

			string configFile = Path.Combine(configPath, "Config.json");
			if (!File.Exists(configFile)) {
				Logger.Log(LogSeverity.Critical, "Main", "Config.json file did not exist. Please add a config file to the RoosterBot folder in %appdata%.");
				Console.ReadKey();
				return;
			}

			ConfigService configService;
			string authToken;
			Dictionary<string, string> schedules;
			try {
				configService = new ConfigService(Path.Combine(configPath, "Config.json"), out authToken, out schedules);
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reading Config.json file.", ex);
				Console.ReadKey();
				return;
			}
			Logger.Log(LogSeverity.Debug, "Main", "Loaded config file");
			#endregion Load config

			#region Start services
			ScheduleService scheduleService = new ScheduleService();
			// Concurrently read schedules.
			Task[] readCSVs = new Task[schedules.Count];
			int i = 0;
			foreach (KeyValuePair<string, string> schedule in schedules) {
				readCSVs[i] = scheduleService.ReadScheduleCSV(schedule.Key, Path.Combine(configPath, schedule.Value));
				i++;
			}
			try {
				Task.WaitAll(readCSVs);
			} catch (Exception ex) {
				Logger.Log(LogSeverity.Critical, "Main", "Error occurred while reading one of the CSV files.", ex);
				Console.ReadKey();
				return;
			}
			Logger.Log(LogSeverity.Debug, "Main", "Loaded schedules");

			m_Commands = new CommandService();
			m_Commands.Log += Logger.LogSync;
			await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly());

			m_Services = new ServiceCollection()
				.AddSingleton(configService)
				.AddSingleton(scheduleService)
				.AddSingleton(m_Commands)
				.BuildServiceProvider();
			Logger.Log(LogSeverity.Debug, "Main", "Started services");
			#endregion Start services

			#region Start client
			m_Client = new DiscordSocketClient(new DiscordSocketConfig() {
				WebSocketProvider = WS4NetProvider.Instance
			});
			m_Client.Log += Logger.LogSync;
			m_Client.MessageReceived += HandleCommand;
			
			await m_Client.LoginAsync(TokenType.Bot, authToken);
			await m_Client.StartAsync();
			#endregion Start client

			#region Quit code
			ProgramState state = ProgramState.BeforeStart;
			m_Client.Ready += () => {
				state = ProgramState.BotRunning;
				return Task.CompletedTask;
			};

			Console.CancelKeyPress += (o, e) => {
				if (state != ProgramState.BotStopped) {
					e.Cancel = true;
					Logger.Log(LogSeverity.Warning, "Main", "Bot is still running. Use Ctrl-Q to stop it, or force-quit this window if it is not responding.");
				}
			};

			ConsoleKeyInfo keyPress;
			do {
				keyPress = Console.ReadKey(true);
			} while (state == ProgramState.BeforeStart || !(keyPress.Modifiers.HasFlag(ConsoleModifiers.Control) && keyPress.Key == ConsoleKey.Q));

			Logger.Log(LogSeverity.Info, "Main", "Ctrl-Q: Stopping bot");
			await m_Client.StopAsync();
			await m_Client.LogoutAsync();
			state = ProgramState.BotStopped;
			Console.WriteLine("Press any key to quit.");
			Console.ReadKey(true);
			#endregion Quit code
		}

		private async Task HandleCommand(SocketMessage messageParam) {
			// Don't process the command if it was a System Message
			if (!(messageParam is SocketUserMessage message))
				return;
			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message is a command, based on if it starts with '!' or a mention prefix
			if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(m_Client.CurrentUser, ref argPos)))
				return;
			// Create a Command Context
			var context = new CommandContext(m_Client, message);
			// Execute the command. (result does not indicate a return value, 
			// rather an object stating if the command executed successfully)
			var result = await m_Commands.ExecuteAsync(context, argPos, m_Services);
			if (!result.IsSuccess)
				await context.Channel.SendMessageAsync(result.ErrorReason);
		}
	}

	public enum ProgramState {
		BeforeStart, BotRunning, BotStopped
	}
}
