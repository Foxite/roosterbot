using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	public class DiscordNotificationHandler {
		public DiscordNotificationHandler(NotificationService notificationService) {
			notificationService.NotificationAdded += SendNotificationAsync;
		}

		private async Task SendNotificationAsync(NotificationEventArgs nea) {
			Logger.Info("DiscordNet", "Sending error report to Discord admins");
			try {
				foreach (SocketUser admin in DiscordNetComponent.Instance.BotAdmins) {
					await (await admin.GetOrCreateDMChannelAsync()).SendMessageAsync(nea.Message);
				}
			} catch (Exception ex) {
				Logger.Error("DiscordNet", "Failed to send error report Discord admins", ex);
			}
		}
	}
}
