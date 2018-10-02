using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System.Collections.Generic;
using System;
using Discord.WebSocket;
using Discord.Commands;

namespace RoosterBot {
	/* The class lets us change our response when a user edits their command (especially after an error occured).
	 * This only works for the last message a user has sent.
	 */
	public class EditedCommandService : CommandService {
		private ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private DiscordSocketClient m_Client;
		private Func<IUserMessage, SocketUserMessage, Task> m_HandleCommandFunction;

		public EditedCommandService(DiscordSocketClient client, Func<IUserMessage, SocketUserMessage, Task> handleCommandFunction) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Client = client;
			m_HandleCommandFunction = handleCommandFunction;

			m_Client.MessageUpdated += OnMessageUpdated;
		}

		// Warning: argument names are speculative because the documentation around the API is nearly nonexistent.
		// The ulong in messageBefore is the id of the IMessage.
		// However, I can only assume that the ISocketMessageChannel was included by mistake/convenience, because you can get the same information from the SocketMessage.
		private async Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (!(messageAfter is SocketUserMessage socketMessageAfter))
				return;

			Console.WriteLine("aa");
			if (m_Messages.TryGetValue(messageAfter.Id, out CommandResponsePair crp)) {
				Console.WriteLine("bb");
				// This part is the point of this class. It checks if the new message is a command, executes it, and tells ReplyAsync in modules that use this
				//  class to edit their response instead of making a new one.
				// This class is designed to work with EditableCmdModuleBase in that aspect. It can simply be replaced as ModuleBase in existing modules and they will
				//  automatically adopt this functionality without needing additional work.
				await m_HandleCommandFunction(crp.Response, socketMessageAfter);
			}
		}

		public void AddResponse(IUserMessage userCommand, IUserMessage botResponse) {
			// Remove previous messages from this user
			IEnumerable<KeyValuePair<ulong, CommandResponsePair>> oldMessages = m_Messages.Where((kvp) => {
				return kvp.Value.Command.Author.Id == userCommand.Author.Id;
			});

			Console.WriteLine(oldMessages == null);
			if (oldMessages.Count() > 0) {
				foreach (KeyValuePair<ulong, CommandResponsePair> item in oldMessages) {
					m_Messages.TryRemove(item.Key, out CommandResponsePair unused);
				}
			}

			m_Messages.TryAdd(userCommand.Id, new CommandResponsePair(userCommand, botResponse));
		}
	}

	public class EditedCommandContext : CommandContext {
		public EditedCommandContext(IDiscordClient client, IUserMessage command, IUserMessage originalResponse) : base(client, command) {
			OriginalResponse = originalResponse;
		}

		// If this is null, we should make a new message.
		public IUserMessage OriginalResponse { get; }
	}

	public struct CommandResponsePair {
		public IUserMessage Command;
		public IUserMessage Response;

		public CommandResponsePair(IUserMessage command, IUserMessage response) {
			Command = command;
			Response = response;
		}
	}
}
