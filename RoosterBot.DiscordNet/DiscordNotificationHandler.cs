using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordNotificationHandler {
		public DiscordNotificationHandler(NotificationService notificationService) {
			notificationService.NotificationAdded += SendNotificationAsync;
		}

		private async Task SendNotificationAsync(NotificationEventArgs nea) {
			Logger.Info("DiscordNet", "Sending error report to bot admin");
			try {
				await (await DiscordNetComponent.Instance.BotOwner.GetOrCreateDMChannelAsync()).SendMessageAsync(nea.Message);
			} catch (Exception ex) {
				Logger.Error("DiscordNet", "Failed to send error report to bot admin", ex);
			}
		}
	}
}
