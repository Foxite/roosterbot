using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Console {
	public abstract class ConsoleSnowflake : ISnowflake {
		public PlatformComponent Platform => ConsoleComponent.Instance;
		public object Id { get; }

		protected ConsoleSnowflake(long id) {
			Id = id;
		}
	}

	public class ConsoleUser : ConsoleSnowflake, IUser {
		public string UserName { get; }
		public string Mention => "@" + UserName;
		public string DisplayName => "ConsoleGuy";

		public ConsoleUser(long id, string name) : base(id) {
			UserName = name;
		}

		public Task<IChannel?> GetPrivateChannel() => Task.FromResult((IChannel?) ConsoleComponent.Instance.TheConsoleChannel);
	}

	public class ConsoleChannel : ConsoleSnowflake, IChannel {
		internal List<ConsoleMessage> m_Messages = new List<ConsoleMessage>();

		public string Name => "Window";

		public ConsoleChannel() : base(3) { }

		public Task<IMessage> GetMessageAsync(object id) => Task.FromResult((IMessage) m_Messages.First(message => message.Id == Id));

		public Task<IMessage> SendMessageAsync(string content, string? filePath = null) {
			var message = new ConsoleMessage(content, true);
			m_Messages.Add(message);
			return Task.FromResult((IMessage) message);
		}
	}

	public class ConsoleMessage : ConsoleSnowflake, IMessage {
		public ConsoleChannel Channel => ConsoleComponent.Instance.TheConsoleChannel;
		public ConsoleUser User { get; }
		public bool SentByRoosterBot { get; }
		public string Content { get; private set; }

		IChannel IMessage.Channel => Channel;
		IUser IMessage.User => User;

		public ConsoleMessage(string content, bool sentByRoosterBot) : base(DateTime.Now.Ticks) {
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
