using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	public sealed class RoosterCommandService : CommandService {
		/// <summary>
		/// Maps message IDs to CommandResponsePairs
		/// </summary>
		private ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private readonly ConfigService m_Config;

		/// <param name="minimumMemorySeconds">How long it takes at least before old commands are deleted. Old commands are not deleted until a new one from the same user comes in.</param>
		internal RoosterCommandService(ConfigService config) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Config = config;
		}

		internal void AddResponse(IUserMessage userCommand, IUserMessage botResponse, string reactionUnicode = null) {
			// Remove previous commands from this user if they are older than the minimum memory
			IEnumerable<ulong> oldCommandIds = m_Messages.Where((kvp) => {
				return (DateTime.Now - kvp.Value.Command.Timestamp.UtcDateTime).TotalSeconds > m_Config.MinimumMemorySeconds && kvp.Value.Command.Author.Id == userCommand.Author.Id;
			}).Select(kvp => kvp.Key);

			foreach (ulong commandId in oldCommandIds) {
				m_Messages.TryRemove(commandId, out CommandResponsePair unused);
			}

			IUserMessage[] newSequence = new IUserMessage[] { botResponse };
			m_Messages.AddOrUpdate(userCommand.Id, new CommandResponsePair(userCommand, newSequence, reactionUnicode), (key, crp) => {
				crp.Responses = crp.Responses.Concat(newSequence).ToArray();
				return crp;
			});
		}

		internal CommandResponsePair GetResponse(IUserMessage userCommand) {
			if (m_Messages.TryGetValue(userCommand.Id, out CommandResponsePair ret)) {
				return ret;
			} else {
				return null;
			}
		}

		internal bool RemoveCommand(ulong commandId, out CommandResponsePair crp) {
			return m_Messages.TryRemove(commandId, out crp);
		}

		internal bool IsMessageCommand(IMessage message, out int argPos) {
			argPos = 0;
			if (message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > m_Config.CommandPrefix.Length &&
				userMessage.HasStringPrefix(m_Config.CommandPrefix, ref argPos)) {
				// First char after prefix
				char firstChar = message.Content.Substring(m_Config.CommandPrefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}
	}

	internal class CommandResponsePair {
		public IUserMessage Command;
		public IUserMessage[] Responses;
		public string ReactionUnicode;

		public CommandResponsePair(IUserMessage command, IUserMessage[] responses, string reactionUnicode = null) {
			Command = command;
			Responses = responses;
			ReactionUnicode = reactionUnicode;
		}
	}
}
