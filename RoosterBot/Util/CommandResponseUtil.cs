using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public static class CommandResponseUtil {
		private const string CommandResponseJsonKey = "command_response";

		public static async Task SetResponseAsync(this UserConfig userConfig, ulong userCommandId, ulong botResponseId) {
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
			} else {
				userConfig.SetData(CommandResponseJsonKey, new[] { new CommandResponsePair(userCommandId, botResponseId) });
			}
			await userConfig.UpdateAsync();
		}

		public static CommandResponsePair? GetResponse(this UserConfig userConfig, ulong messageId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.CommandId == messageId);
			} else {
				return null;
			}
		}

		public static async Task<CommandResponsePair?> RemoveCommandAsync(this UserConfig userConfig, ulong commandId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				for (int i = 0; i < crps.Count; i++) {
					if (crps[i].CommandId == commandId) {
						CommandResponsePair ret = crps[i];
						crps.RemoveAt(i);
						await userConfig.UpdateAsync();
						return ret;
					}
				}
				return null;
			} else {
				return null;
			}
		}

		public static Task SetResponseAsync(this UserConfig userConfig, IUserMessage userMessage, IUserMessage botResponse) => SetResponseAsync(userConfig, userMessage.Id, botResponse.Id);
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, IUserMessage message) => GetResponse(userConfig, message.Id);
		public static Task<CommandResponsePair?> RemoveCommandAsync(this UserConfig userConfig, IUserMessage command) => RemoveCommandAsync(userConfig, command.Id);
	}
}
