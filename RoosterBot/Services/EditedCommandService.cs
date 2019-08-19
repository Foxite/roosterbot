using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot.Services {
	/* The class lets us change our response when a user edits their command (especially after an error occured).
	 * This only works for the last message a user has sent.
	 * 
	 * It's easily possible to make this work for all messages a user has sent, but that would cause unlimited rise in memory usage, and we only have 1GB to work with on EC2.
	 * However, we could periodically remove messages older than a certain age, like five minutes.
	 */
	public class EditedCommandService : RoosterCommandService {
		private ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private DiscordSocketClient m_Client;

		public event Func<IUserMessage, IUserMessage, Task> CommandEdited;

		public EditedCommandService(DiscordSocketClient client) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Client = client;

			m_Client.MessageUpdated += OnMessageUpdated;
			m_Client.MessageDeleted += OnMessageDeleted;
		}

		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel) {
			if (m_Messages.TryRemove(message.Id, out CommandResponsePair crp)) {
				await crp.Response.DeleteAsync();
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
			if (!(messageAfter is SocketUserMessage socketMessageAfter))
				return;

			if (m_Messages.TryGetValue(messageAfter.Id, out CommandResponsePair crp)) {
				await CommandEdited?.Invoke(crp.Response, socketMessageAfter);
			}
		}

		public void AddResponse(IUserMessage userCommand, IUserMessage botResponse, string reactionUnicode = null) {
			// Remove previous messages from this user
			IEnumerable<KeyValuePair<ulong, CommandResponsePair>> oldMessages = m_Messages.Where((kvp) => {
				return kvp.Value.Command.Author.Id == userCommand.Author.Id;
			});

			if (oldMessages.Count() > 0) {
				foreach (KeyValuePair<ulong, CommandResponsePair> item in oldMessages) {
					m_Messages.TryRemove(item.Key, out CommandResponsePair unused);
				}
			}

			m_Messages.TryAdd(userCommand.Id, new CommandResponsePair(userCommand, botResponse, reactionUnicode));
		}

		public CommandResponsePair GetResponse(IUserMessage userCommand) {
			if (m_Messages.TryGetValue(userCommand.Id, out CommandResponsePair ret)) {
				return ret;
			} else {
				return default(CommandResponsePair);
			}
		}
	}

	public class EditedCommandContext : CommandContext {
		public EditedCommandContext(IDiscordClient client, IUserMessage command, IUserMessage originalResponse) : base(client, command) {
			OriginalResponse = originalResponse;
		}

		// If this is null, we should make a new message.
		public IUserMessage OriginalResponse { get; }
	}

	public class CommandResponsePair {
		public IUserMessage Command;
		public IUserMessage Response;
		public string ReactionUnicode;

		public CommandResponsePair(IUserMessage command, IUserMessage response, string reactionUnicode = null) {
			Command = command;
			Response = response;
			ReactionUnicode = reactionUnicode;
		}
	}
}
