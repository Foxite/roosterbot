using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class RestartHandler {
		private int m_Attempts;
		private NotificationService m_Notifications;
		private Exception m_InitialException;

		public int MaxAttempts { get; }

		public RestartHandler(DiscordSocketClient discord, NotificationService notif, int maxAttempts) {
			m_Attempts = 0;
			MaxAttempts = maxAttempts;
			m_Notifications = notif;

			discord.Disconnected += async (e) => {
				if (m_Attempts == 0) {
					m_InitialException = e;
				}

				m_Attempts++;

				if (m_Attempts > MaxAttempts) {
					await Restart(e);
				}
			};

			discord.Connected += () => {
				m_Attempts = 0;
				m_InitialException = null;
				return Task.CompletedTask;
			};
		}

		private async Task Restart(Exception e) {
			string report = $"RoosterBot has failed to reconnect after {m_Attempts} attempts.\n\n";
			if (m_InitialException != null) {
				report += $"The initial exception is: {m_InitialException.ToStringDemystified()}";
			} else {
				report += "No initial exception was attached.\n\n";
			}

			if (e == null) {
				report += $"The last exception is: {e.ToStringDemystified()}";
			} else {
				report += "No last exception was attached.\n\n";
			}

			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await m_Notifications.AddNotificationAsync(report);

			Program.Instance.Restart();
		}
	}
}
