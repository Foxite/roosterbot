/* // TODO Still necessary?
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot {
	/// <summary>
	/// This handler will make sure the bot restarts after more than a specified amount of connection attempts, if the connection is lost.
	/// </summary>
	internal sealed class RestartHandler : RoosterHandler {
		public NotificationService Notifications { get; set; } = null!;
		public DiscordSocketClient Discord { get; set; } = null!;

		private readonly int m_MaxAttempts;

		private int m_Attempts;
		private Exception? m_InitialException;

		public RestartHandler(IServiceProvider isp, int maxAttempts) : base(isp) {
			m_Attempts = 0;
			m_MaxAttempts = maxAttempts;

			Discord.Disconnected += async (e) => {
				if (m_Attempts == 0) {
					m_InitialException = e;
				}

				m_Attempts++;

				if (m_Attempts > m_MaxAttempts) {
					await Restart(e);
				}
			};

			Discord.Connected += () => {
				m_Attempts = 0;
				m_InitialException = null;
				return Task.CompletedTask;
			};
		}

		private async Task Restart(Exception? e) {
			string report = $"RoosterBot has failed to reconnect after {m_Attempts} attempts.\n\n";
			if (m_InitialException != null) {
				report += $"The initial exception is: {m_InitialException.ToStringDemystified()}";
			} else {
				report += "No initial exception was attached.\n\n";
			}

			if (e != null) {
				report += $"The last exception is: {e.ToStringDemystified()}";
			} else {
				report += "No last exception was attached.\n\n";
			}

			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await Notifications.AddNotificationAsync(report);

			Program.Instance.Restart();
		}
	}
}
*/