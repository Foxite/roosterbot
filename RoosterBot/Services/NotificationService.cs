using System;
using System.Threading.Tasks;
using Qommon.Events;

namespace RoosterBot {
	/// <summary>
	/// Allows you to notify the bot administrators of an event.
	/// You cannot send notifications yourself, but you can provide delivery target for the notification.
	/// </summary>
	public sealed class NotificationService {
		private readonly AsynchronousEvent<NotificationEventArgs> m_NotificationAddedEvent = new();

		/// <summary>
		/// Fired when a notification is emitted by RoosterBot.
		/// </summary>
		public event AsynchronousEventHandler<NotificationEventArgs> NotificationAdded {
			add => m_NotificationAddedEvent.Hook(value);
			remove => m_NotificationAddedEvent.Unhook(value);
		}

		internal NotificationService() { }

		internal Task AddNotificationAsync(string message) {
			return m_NotificationAddedEvent.InvokeAsync(new NotificationEventArgs(message));
		}
	}

	/// <summary>
	/// Event arguments for <see cref="NotificationService.NotificationAdded"/>.
	/// </summary>
	public class NotificationEventArgs : EventArgs {
		/// <summary>
		/// The notification text.
		/// </summary>
		public string Message { get; }

		internal NotificationEventArgs(string message) {
			Message = message;
		}
	}
}
