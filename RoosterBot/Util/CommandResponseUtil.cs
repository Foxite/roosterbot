using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot {
	public static class CommandResponseUtil {
		private const string CommandResponseJsonKey = "command_response";

		public static void SetResponse(this UserConfig userConfig, SnowflakeReference userCommand, SnowflakeReference botResponse) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				CommandResponsePair? relevantCRP = crps.SingleOrDefault(crp => crp.Command == userCommand);
				if (relevantCRP == null) {
					crps.Add(new CommandResponsePair(userCommand, botResponse));

					// Remove old commands
					if (crps.Count > 5) {
						crps.RemoveRange(0, crps.Count - 5);
					}
				} else {
					relevantCRP.Response = botResponse;
				}
				userConfig.SetData(CommandResponseJsonKey, crps);
			} else {
				userConfig.SetData(CommandResponseJsonKey, new[] { new CommandResponsePair(userCommand, botResponse) });
			}
		}

		public static CommandResponsePair? GetResponse(this UserConfig userConfig, SnowflakeReference message) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.Command == message);
			} else {
				return null;
			}
		}

		public static CommandResponsePair? RemoveCommand(this UserConfig userConfig, SnowflakeReference command) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				for (int i = 0; i < crps.Count; i++) {
					if (crps[i].Command == command) {
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

		public static void SetResponse(this UserConfig userConfig, IMessage userMessage, IMessage botResponse) => SetResponse(userConfig, userMessage.GetReference(), botResponse.GetReference());
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, IMessage message) => GetResponse(userConfig, message.GetReference());
	}
}
