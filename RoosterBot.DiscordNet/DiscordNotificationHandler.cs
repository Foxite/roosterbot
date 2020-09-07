using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordNotificationHandler {
		public DiscordNotificationHandler(NotificationService notificationService) {
			notificationService.NotificationAdded += SendNotificationAsync;
		}

		private async Task SendNotificationAsync(NotificationEventArgs nea) {
			Logger.Info("SNSService", "Sending error report to SNS");
			try {
				await (await DiscordNetComponent.Instance.BotOwner.GetOrCreateDMChannelAsync()).SendMessageAsync(nea.Message);
			} catch (Exception ex) {
				Logger.Error("SNSService", "Failed to send error report to SNS", ex);
			}
		}
	}
}
