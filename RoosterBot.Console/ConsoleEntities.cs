using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Console {
	public class ConsoleUser : IUser {
		public string UserName { get; }
		public object Id { get; }
		public string Mention => "@" + UserName;
		public string Platform => "Console";
		public string DisplayName => "ConsoleGuy";

		public ConsoleUser(object id, string name) {
			UserName = name;
			Id = id;
		}

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
		public ConsoleUser User { get; }
		public object Id { get; } = DateTime.Now.Ticks;
		public string Platform => "Console";
		public bool SentByRoosterBot { get; }
		public string Content { get; private set; }

		IChannel IMessage.Channel => Channel;
		IUser IMessage.User => User;

		public ConsoleMessage(string content, bool sentByRoosterBot) {
			Content = content;
			SentByRoosterBot = sentByRoosterBot;
			User = SentByRoosterBot ? ConsoleComponent.Instance.ConsoleBotUser : ConsoleComponent.Instance.TheConsoleUser;
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
