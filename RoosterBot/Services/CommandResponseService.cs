using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	public class CommandResponseService {
		/// <summary>
		/// Maps command IDs to CommandResponsePairs
		/// </summary>
		private readonly ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private readonly ConfigService m_Config;

		/// <param name="minimumMemorySeconds">How long it takes at least before old commands are deleted. Old commands are not deleted until a new one from the same user comes in.</param>
		internal CommandResponseService(ConfigService config) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Config = config;
		}

		public void AddResponse(IUserMessage userCommand, IUserMessage botResponse) {
			// Remove previous commands from this user if they are older than the minimum memory
			IEnumerable<ulong> oldCommandIds = m_Messages.Where((kvp) => {
				return (DateTime.Now - kvp.Value.Command.Timestamp.UtcDateTime).TotalSeconds > m_Config.MinimumMemorySeconds && kvp.Value.Command.Author.Id == userCommand.Author.Id;
			}).Select(kvp => kvp.Key);

			foreach (ulong commandId in oldCommandIds) {
				m_Messages.TryRemove(commandId, out CommandResponsePair unused);
			}

			IUserMessage[] newSequence = new IUserMessage[] { botResponse };
			m_Messages.AddOrUpdate(userCommand.Id, new CommandResponsePair(userCommand, newSequence), (key, crp) => {
				((ICommandResponsePair) crp).Responses = crp.Responses.Concat(newSequence).ToArray();
				return crp;
			});
		}

		public void ModifyResponse(ulong commandId, IUserMessage[] responses) {
			((ICommandResponsePair) GetResponse(commandId)).Responses = responses;
		}

		public CommandResponsePair GetResponse(ulong userCommandId) {
			if (m_Messages.TryGetValue(userCommandId, out CommandResponsePair ret)) {
				return ret;
			} else {
				return null;
			}
		}

		public bool RemoveCommand(ulong commandId, out CommandResponsePair crp) {
			return m_Messages.TryRemove(commandId, out crp);
		}

		public CommandResponsePair GetResponse(IUserMessage userCommand) => GetResponse(userCommand.Id);
		public bool RemoveCommand(IUserMessage command, out CommandResponsePair crp) => RemoveCommand(command.Id, out crp);
		public void ModifyResponse(IUserMessage command, IUserMessage[] responses) => ModifyResponse(command.Id, responses);
	}

	internal interface ICommandResponsePair {
		IUserMessage Command { set; }
		IUserMessage[] Responses { set; }
	}

	public class CommandResponsePair : ICommandResponsePair {
		IUserMessage   ICommandResponsePair.Command { set => Command = value; }
		IUserMessage[] ICommandResponsePair.Responses { set => Responses = value; }

		public IUserMessage Command { get; private set; }
		public IUserMessage[] Responses { get; private set; }

		public CommandResponsePair(IUserMessage command, IUserMessage[] responses) {
			Command = command;
			Responses = responses;
		}
	}
}
