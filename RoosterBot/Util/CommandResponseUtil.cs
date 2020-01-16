using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// A static class containing some helper functions for dealing with <see cref="CommandResponsePair"/>s.
	/// </summary>
	public static class CommandResponseUtil {
		private const string CommandResponseJsonKey = "command_response";

		/// <summary>
		/// Set the response to the message indicated by <paramref name="userCommand"/> to the message indicated by <paramref name="botResponse"/>.
		/// </summary>
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

		/// <summary>
		/// Retrieve the bot's response to the message indicated by <paramref name="message"/>.
		/// </summary>
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, SnowflakeReference message) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.Command == message);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Delete a <see cref="CommandResponsePair"/> for a command indicated by <paramref name="command"/> from a <see cref="UserConfig"/>.
		/// </summary>
		/// <returns>The deleted <see cref="CommandResponsePair"/>.</returns>
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

		/// <summary>
		/// Set the response to a command to an <see cref="IMessage"/> sent by RoosterBot.
		/// </summary>
		public static void SetResponse(this UserConfig userConfig, IMessage userMessage, IMessage botResponse) => SetResponse(userConfig, userMessage.GetReference(), botResponse.GetReference());

		/// <summary>
		/// Get RoosterBot's response to an <see cref="IMessage"/> sent by a user.
		/// </summary>
		public static CommandResponsePair? GetResponse(this UserConfig userConfig, IMessage message) => GetResponse(userConfig, message.GetReference());
	}
}
