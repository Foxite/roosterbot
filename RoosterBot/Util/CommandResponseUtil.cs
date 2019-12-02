﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public static class CommandResponseUtil {
		private const string CommandResponseJsonKey = "command_response";

		public static void SetResponse(this UserConfig userConfig, ulong userCommandId, ulong botResponseId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				CommandResponsePair? relevantCRP = crps.SingleOrDefault(crp => crp.CommandId == userCommandId);
				if (relevantCRP == null) {
					crps.Add(new CommandResponsePair(userCommandId, botResponseId));

					// Remove old commands
					// TODO (feature) The amount of commands that are remembered needs to be configurable
					// Previously we would not remember commands older than a configured time period, but now that these methods are static we don't have a ConfigService reference anymore.
					// For now I've hardcoded the amount to be 5.
					if (crps.Count > 5) {
						crps.RemoveRange(0, crps.Count - 5);
					}
				} else {
					relevantCRP.ResponseId = botResponseId;
				}
				userConfig.SetData(CommandResponseJsonKey, crps);
			} else {
				userConfig.SetData(CommandResponseJsonKey, new[] { new CommandResponsePair(userCommandId, botResponseId) });
			}
		}

		public static CommandResponsePair? GetResponse(this UserConfig userConfig, ulong messageId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.CommandId == messageId);
			} else {
				return null;
			}
		}

		public static CommandResponsePair? RemoveCommand(this UserConfig userConfig, ulong commandId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				for (int i = 0; i < crps.Count; i++) {
					if (crps[i].CommandId == commandId) {
						CommandResponsePair ret = crps[i];
						crps.RemoveAt(i);
						return ret;
					}
				}
				return null;
			} else {
				return null;
			}
		}

		public static void SetResponse(this UserConfig userConfig, IUserMessage userMessage, IUserMessage botResponse) => SetResponse(userConfig, userMessage.Id, botResponse.Id);
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, IUserMessage message) => GetResponse(userConfig, message.Id);
		public static CommandResponsePair? RemoveCommand(this UserConfig userConfig, IUserMessage command) => RemoveCommand(userConfig, command.Id);

		// TODO (feature) Certain commands should opt out from command editing/deletion, most notably commands that can upload files
		// Recommend doing this with an attribute and saving the Command with the CommandResponsePair (good luck serializing it)
		public static async Task<IUserMessage> RespondAsync(this RoosterCommandContext context, string message, string? filePath = null) {
			CommandResponsePair? crp = context.UserConfig.GetResponse(context.Message);
			IUserMessage? response = crp == null ? null : (IUserMessage) await context.Channel.GetMessageAsync(crp.ResponseId);
			if (response == null) {
				// The response was already deleted, or there was no response to begin with.
				if (filePath == null) {
					response = await context.Channel.SendMessageAsync(message);
				} else {
					response = await context.Channel.SendFileAsync(filePath, message);
				}
				SetResponse(context.UserConfig, context.Message, response);
			} else {
				// The command was edited.
				await response.ModifyAsync(props => {
					props.Content = message;
				});
			}

			return response;
		}
	}
}
