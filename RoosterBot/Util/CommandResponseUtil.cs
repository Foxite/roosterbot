using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot {
	public static class CommandResponseUtil {
		private const string CommandResponseJsonKey = "command_response";

		public static void SetResponse(this UserConfig userConfig, object userCommandId, object botResponseId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				CommandResponsePair? relevantCRP = crps.SingleOrDefault(crp => crp.CommandId == userCommandId);
				if (relevantCRP == null) {
					crps.Add(new CommandResponsePair(userCommandId, botResponseId));

					// Remove old commands
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

		public static CommandResponsePair? GetResponse(this UserConfig userConfig, object messageId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.CommandId == messageId);
			} else {
				return null;
			}
		}

		public static CommandResponsePair? RemoveCommand(this UserConfig userConfig, object commandId) {
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

		public static void SetResponse(this UserConfig userConfig, IMessage userMessage, IMessage botResponse) => SetResponse(userConfig, userMessage.Id, botResponse.Id);
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, IMessage message) => GetResponse(userConfig, message.Id);
		public static CommandResponsePair? RemoveCommand(this UserConfig userConfig, IMessage command) => RemoveCommand(userConfig, command.Id);

		public static async Task<IMessage> RespondAsync(this RoosterCommandContext context, string message, string? filePath = null) {
			CommandResponsePair? crp = context.UserConfig.GetResponse(context.Message);
			IMessage? response = crp == null ? null : await context.Channel.GetMessageAsync(crp.ResponseId);
			if (response == null) {
				// The response was already deleted, or there was no response to begin with.
				response = await context.Channel.SendMessageAsync(message, filePath);
				SetResponse(context.UserConfig, context.Message, response);
			} else {
				// The command was edited.
				await response.ModifyAsync(message, filePath);
			}

			return response;
		}
	}
}
