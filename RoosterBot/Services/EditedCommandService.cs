using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot {
	/* The class lets us change our response when a user edits their command (especially after an error occured).
	 * This only works for the last message a user has sent.
	 * 
	 * It's easily possible to make this work for all messages a user has sent, but that would cause unlimited rise in memory usage, and we only have 1GB to work with on EC2.
	 * However, we could periodically remove messages older than a certain age, like five minutes.
	 */
	// TODO all responses should be deleted when the command is deleted, even if the module does not derive from EditableCmdModuleBase
	public class EditedCommandService : CommandService {
		/// <summary>
		/// Maps message IDs to CommandResponsePairs
		/// </summary>
		private ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private DiscordSocketClient m_Client;

		public event Func<CommandResponsePair, Task> CommandEdited;

		internal EditedCommandService(DiscordSocketClient client) : this(client, new CommandServiceConfig()) { }

		internal EditedCommandService(DiscordSocketClient client, CommandServiceConfig config) : base(config) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Client = client;

			m_Client.MessageUpdated += OnMessageUpdated;
			m_Client.MessageDeleted += OnMessageDeleted;
		}

		// Delete responses when commands are deleted
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, IMessageChannel channel) {
			if (m_Messages.TryRemove(message.Id, out CommandResponsePair crp)) {
				await Util.DeleteAll(channel, crp.Responses);
			}
		}

		// Warning: argument names are speculative because the documentation around the API is nearly nonexistent.
		// The ulong in messageBefore is the id of the IMessage.
		// However, I can only assume that the ISocketMessageChannel was included by mistake/convenience, because you can get the same information from the SocketMessage.
		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			// This part is the point of this class. It checks if the new message is a command, executes it, and tells ReplyAsync in modules that use this
			//  class to edit their response instead of making a new one.
			// This class is designed to work with EditableCmdModuleBase in that aspect. It can simply be replaced as ModuleBase in existing modules and they will
			//  automatically adopt this functionality without needing additional work.
			if (!(messageAfter is SocketUserMessage userMessageAfter))
				return;

			if (m_Messages.TryGetValue(messageAfter.Id, out CommandResponsePair crp)) {
				await Util.InvokeAsyncEventConcurrent(CommandEdited, crp, userMessageAfter);
			}
		}

		public void AddResponse(IUserMessage userCommand, IUserMessage botResponse, string reactionUnicode = null) {
			// Remove previous messages from this user
			IEnumerable<KeyValuePair<ulong, CommandResponsePair>> oldMessages = m_Messages.Where((kvp) => {
				return kvp.Value.Command.Author.Id == userCommand.Author.Id;
			});

			foreach (KeyValuePair<ulong, CommandResponsePair> item in oldMessages) {
				m_Messages.TryRemove(item.Key, out CommandResponsePair unused);
			}

			m_Messages[userCommand.Id] = new CommandResponsePair(userCommand, new IUserMessage[] { botResponse }, reactionUnicode);
		}

		public CommandResponsePair GetResponse(IUserMessage userCommand) {
			if (m_Messages.TryGetValue(userCommand.Id, out CommandResponsePair ret)) {
				return ret;
			} else {
				return null;
			}
		}
	}

	public class CommandResponsePair {
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
