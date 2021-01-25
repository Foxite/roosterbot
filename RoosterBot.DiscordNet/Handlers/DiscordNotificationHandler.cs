using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class DiscordNotificationHandler {
		public DiscordNotificationHandler(NotificationService notificationService) {
			notificationService.NotificationAdded += SendNotificationAsync;
		}
		private async Task SendNotificationAsync(NotificationEventArgs nea) {
			Logger.Info(DiscordNetComponent.LogTag, "Sending error report to Discord admins");
			try {
				foreach (SocketUser admin in DiscordNetComponent.Instance.BotAdmins) {
					await (await admin.GetOrCreateDMChannelAsync()).SendMessageAsync(nea.Message);
				}
			} catch (Exception ex) {
				Logger.Error(DiscordNetComponent.LogTag, "Failed to send error report Discord admins", ex);
			}
		}
	}
}
