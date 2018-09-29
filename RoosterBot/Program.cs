using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using System.Reflection;

/* Development log (in lieu of git):
 * 27-09-18 16:30: Started work. Building on top of the EchoBot to skip basic bot construction.
 * 27-09-18 20:58: Got basics done, it can tells you what class you have now and where and by who, and what comes after this. same thing for any teacher.
 * 27-09-18 23:03: Can now look up teachers by any name, including their abbreviation, their first name, and alternative spellings of their name. this is hardcoded for now.
 * 28-09-18      : Can now look up rooms.
 * 28-09-18      : Better error handling, and now adds a reaction when an error occurs.
 * 28-09-18      : Code now a bit more organized.
 * 29-09-18 11:00: Bot should now be able to determine a student's class based on their ranks. I need more users in the testing server to do a proper test, though.
 * 29-09-18 11:58: Dropped that feature. It isn't that much more practical than giving it your class, and it's not particularly easy to do in the code.
 * 29-09-18 12:04: Renamed project to RoosterBot (because that's what it is now).
 * 20-09-18 13:18: Investigating Amazon EC2 as a potential VPS for this bot. This means that I'm probably going to integrate this with their libraries later on.
 */

namespace RoosterBot {
	public class Program {
		public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		private DiscordSocketClient m_Client;
		private IServiceProvider m_Services;
		private CommandService m_Commands;

		public async Task MainAsync() {
			Logger.Log(LogSeverity.Info, "Main", "Starting bot");

			m_Client = new DiscordSocketClient();
			m_Client.Log += Logger.LogSync;
			m_Commands = new CommandService();
			m_Commands.Log += Logger.LogSync;

			ScheduleService scheduleService = new ScheduleService();
			Task.WaitAll(scheduleService.ReadScheduleCSV("StudentSets", "leerlingen.csv"),
						 scheduleService.ReadScheduleCSV("StaffMember", "leraren.csv"),
						 scheduleService.ReadScheduleCSV("Room", "lokalen.csv"));

			m_Services = new ServiceCollection()
				.AddSingleton(new ConfigService(2f))
				.AddSingleton(scheduleService)
				.BuildServiceProvider();

			m_Client.MessageReceived += HandleCommand;
			await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly());
			
			string token = "NDMwNjk2Nzk5Mzk3NzQwNTQ1.Doe-JA.rvM99trs3ri5jVmlGCxdOC-Yyjk";
			// permissions integer: 3147776 (send text messages, connect and speak to voice)
			// client id: 430696799397740545
			// client secret: pZzWaV8knPqMPIDSWdBCJh8FRE3PuEJU
			// https://discordapp.com/oauth2/authorize?client_id=430696799397740545&scope=bot

			await m_Client.LoginAsync(TokenType.Bot, token);
			await m_Client.StartAsync();

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
