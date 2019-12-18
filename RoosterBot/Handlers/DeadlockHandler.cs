/* // TODO Still necessary? Hasn't been triggered in months
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace RoosterBot {
	/// <summary>
	/// This handler makes sure the bot will restart if it is disconnected for more than a specified time.
	/// </summary>
	internal sealed class DeadlockHandler : RoosterHandler {
		public NotificationService NotificationService { get; set; } = null!;
		public DiscordSocketClient Client { get; set; } = null!;

		private readonly int m_MaxDisconnectMillis;

		private Timer? m_Timer;
		private bool m_TimerRunning;

		public DeadlockHandler(IServiceProvider isp, int maxDisconnectMillis) : base(isp) {
			m_MaxDisconnectMillis = maxDisconnectMillis;

			Client.Disconnected += (e) => {
				if (m_Timer != null) {
					m_Timer.Dispose();
					m_Timer = null;
				}
				m_Timer = new Timer(TimerCallback, e, m_MaxDisconnectMillis, -1);
				m_TimerRunning = true;
				return Task.CompletedTask;
			};

			Client.Connected += () => {
				m_TimerRunning = false;
				if (m_Timer != null) {
					m_Timer.Dispose();
					m_Timer = null;
				}

				return Task.CompletedTask;
			};
		}

		private async void TimerCallback(object? state) {
			if (m_TimerRunning) { // Prevent race condition. Apparently Timer may call this function after we stopped it, and we have to account for that.
				await Restart(state as Exception);
			}
		}

		private async Task Restart(Exception? e) {
			string report = $"RoosterBot has failed to reconnect after {m_MaxDisconnectMillis / 1000} seconds.\n\n";

			if (e != null) {
				report += $"The exception is: {e.ToString()}";
			} else {
				report += "No exception was attached.\n\n";
			}

			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await NotificationService.AddNotificationAsync(report);

			Process.Start(new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, @"..\AppStart\AppStart.exe"), "delay 20000"));
			Program.Instance.Shutdown();
		}
	}
}
*/