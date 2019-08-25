﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class CommandHandler : IDisposable {
		private EditedCommandService m_Commands;
		private ConfigService m_ConfigService;
		private DiscordSocketClient m_Client;

		internal CommandHandler(IServiceCollection serviceCollection, ConfigService configService, DiscordSocketClient client) {
			m_ConfigService = configService;
			m_Client = client;

			m_Client.MessageReceived += HandleNewCommand;

			m_Commands = new EditedCommandService(m_Client);
			m_Commands.Log += Logger.LogSync;
			m_Commands.CommandEdited += HandleEditedCommand;
			m_Commands.CommandExecuted += OnCommandExecuted;

			serviceCollection.AddSingleton(m_Commands);
		}

		/// <summary>
		/// Executes a command according to specified string input, regardless of the actual content of the message.
		/// </summary>
		/// <param name="calltag">Used for debugging. This identifies where this call originated.</param>
		public async Task ExecuteSpecificCommand(IUserMessage initialResponse, string specificInput, IUserMessage message, string calltag) {
			EditedCommandContext context = new EditedCommandContext(m_Client, message, initialResponse, calltag);

			Logger.Debug("Main", $"Executing specific input `{specificInput}` with calltag `{calltag}`");

			await m_Commands.ExecuteAsync(context, specificInput, Program.Instance.Components.Services);
		}

		private async Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (IsMessageCommand(socketMessage, out int argPos)) {
				EditedCommandContext context = new EditedCommandContext(m_Client, socketMessage as IUserMessage, null);

				await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services);
			}
		}

		private async Task HandleEditedCommand(IUserMessage ourResponse, IUserMessage command) {
			if (IsMessageCommand(command, out int argPos)) {
				EditedCommandContext context = new EditedCommandContext(m_Client, command, ourResponse);

				await m_Commands.ExecuteAsync(context, argPos, Program.Instance.Components.Services);
			} else {
				await ourResponse.DeleteAsync();
			}
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

		private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result) {
			if (!result.IsSuccess) {
				string response = null;
				bool bad = false;
				string badReport = $"\"{context.Message}\": ";

				if (result.Error.HasValue) {
					switch (result.Error.Value) {
						case CommandError.UnknownCommand:
							response = string.Format(Resources.Program_OnCommandExecuted_UnknownCommand, m_ConfigService.CommandPrefix);
							break;
						case CommandError.BadArgCount:
							response = Resources.Program_OnCommandExecuted_BadArgCount;
							break;
						case CommandError.UnmetPrecondition:
							response = result.ErrorReason;
							break;
						case CommandError.ParseFailed:
							response = Resources.Program_OnCommandExecuted_ParseFailed;
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

					response = Resources.RoosterBot_FatalError;
				}

				IUserMessage initialResponse = (context as EditedCommandContext)?.OriginalResponse;
				if (initialResponse == null) {
					m_Commands.AddResponse(context.Message, await context.Channel.SendMessageAsync(response));
				} else {
					await initialResponse.ModifyAsync((msgProps) => { msgProps.Content = response; });
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					((IDisposable) m_Commands).Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
