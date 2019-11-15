using Discord;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot {
	// TODO (refactor) This service is superseded by the new UserConfigService
	public sealed class CommandResponseService {
		private const string CommandResponseJsonKey = "command_response";

		private readonly ConfigService m_Config;

		internal CommandResponseService(ConfigService config) {
			m_Config = config;
		}

		public async Task SetResponseAsync(UserConfig userConfig, IUserMessage userCommand, IUserMessage botResponse) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				CommandResponsePair? relevantCRP = crps.SingleOrDefault(crp => crp.Command.Id == userCommand.Id);
				if (relevantCRP == null) {
					crps.Add(new CommandResponsePair(userCommand, botResponse));
				} else {
					relevantCRP.Response = botResponse;
				}
			} else {
				userConfig.SetData(CommandResponseJsonKey, new[] { new CommandResponsePair(userCommand, botResponse) });
			}
			await userConfig.UpdateAsync();
		}

		public CommandResponsePair? GetResponse(UserConfig userConfig, ulong messageId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out CommandResponsePair[]? crps)) {
				return crps.SingleOrDefault(crp => crp.Command.Id == messageId);
			} else {
				return null;
			}
		}

		public async Task<CommandResponsePair?> RemoveCommandAsync(UserConfig userConfig, ulong commandId) {
			if (userConfig.TryGetData(CommandResponseJsonKey, out List<CommandResponsePair>? crps)) {
				for (int i = 0; i < crps.Count; i++) {
					if (crps[i].Command.Id == commandId) {
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

		public CommandResponsePair? GetResponse(IUserMessage userCommand) => GetResponse(userCommand.Id);
		public bool RemoveCommand(IUserMessage command, [NotNullWhen(true)] out CommandResponsePair? crp) => RemoveCommand(command.Id, out crp);
		public void ModifyResponse(IUserMessage command, IUserMessage[] responses) => ModifyResponse(command.Id, responses);
	}

	public class CommandResponsePair {
		public IUserMessage Command { get; internal set; }
		public IUserMessage Response { get; internal set; }

		public CommandResponsePair(IUserMessage command, IUserMessage response) {
			Command = command;
			Response = response;
		}
	}
}
