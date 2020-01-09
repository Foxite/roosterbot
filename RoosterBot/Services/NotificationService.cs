using System;
using System.Threading.Tasks;
using Qommon.Events;

namespace RoosterBot {
	public sealed class NotificationService {
		private readonly AsynchronousEvent<NotificationEventArgs> m_NotificationAddedEvent = new AsynchronousEvent<NotificationEventArgs>();

		public event AsynchronousEventHandler<NotificationEventArgs> NotificationAdded {
			add => m_NotificationAddedEvent.Hook(value);
			remove => m_NotificationAddedEvent.Unhook(value);
		}

		internal NotificationService() { }

		internal Task AddNotificationAsync(string message) {
			return m_NotificationAddedEvent.InvokeAsync(new NotificationEventArgs(message));
		}
	}

	public class NotificationEventArgs : EventArgs {
		public string Message { get; }

		public NotificationEventArgs(string message) {
			Message = message;
		}
	}
}
