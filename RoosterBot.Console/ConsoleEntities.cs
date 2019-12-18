using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Console {
	public class ConsoleUser : IUser {
		public string Name => "User";
		public string Mention => "@User";
		public object Id => 1;
		public string Platform => "Console";

		public Task<IChannel?> GetPrivateChannel() => Task.FromResult((IChannel?) ConsoleComponent.Instance.TheConsoleChannel);
	}

	public class ConsoleChannel : IChannel {
		internal List<ConsoleMessage> m_Messages = new List<ConsoleMessage>();

		public string Name => "Window";
		public object Id => 2;
		public string Platform => "Console";

		public Task<IMessage> GetMessageAsync(object id) => Task.FromResult((IMessage) m_Messages.First(message => message.Id == Id));

		public Task<IMessage> SendMessageAsync(string content, string? filePath = null) {
			var message = new ConsoleMessage(content, true);
			m_Messages.Add(message);
			return Task.FromResult((IMessage) message);
		}
	}

	public class ConsoleMessage : IMessage {
		public ConsoleChannel Channel => ConsoleComponent.Instance.TheConsoleChannel;
		public ConsoleUser User => ConsoleComponent.Instance.TheConsoleUser;
		public object Id { get; } = DateTime.Now.Ticks;
		public string Platform => "Console";
		public bool SentByRoosterBot { get; }
		public string Content { get; private set; }

		IChannel IMessage.Channel => Channel;
		IUser IMessage.User => User;

		public ConsoleMessage(string content, bool sentByRoosterBot) {
			Content = content;
			SentByRoosterBot = sentByRoosterBot;
		}

		public Task DeleteAsync() {
			Channel.m_Messages.Remove(this);
			return Task.CompletedTask;
		}

		public Task ModifyAsync(string newContent, string? filePath = null) {
			Content = newContent;
			return Task.CompletedTask;
		}
	}
}
