using System;
using System.Threading.Tasks;

namespace RoosterBot {
	public class NotificationService {
		public event Func<string, Task> NotificationAdded;

		internal NotificationService() { }

		internal async Task AddNotificationAsync(string message) {
			await NotificationAdded?.Invoke(message);
		}
	}
}
