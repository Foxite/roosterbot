using System;
using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class NotificationService {
		public event Func<string, Task>? NotificationAdded;

		internal NotificationService() { }

		internal async Task AddNotificationAsync(string message) {
			if (!(NotificationAdded is null)) {
				await DelegateUtil.InvokeAsyncEventConcurrent(NotificationAdded, message);
			}
		}
	}
}
