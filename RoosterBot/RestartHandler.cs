using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoosterBot.Services;

namespace RoosterBot {
	internal sealed class RestartHandler {
		private int m_Attempts;
		private SNSService m_SNS;

		public int MaxAttempts { get; }

		public RestartHandler(DiscordSocketClient discord, SNSService sns, ConfigService config, int maxAttempts) {
			m_Attempts = 0;
			MaxAttempts = maxAttempts;
			m_SNS = sns;

			discord.Disconnected += async (e) => {
				m_Attempts++;

				if (m_Attempts > MaxAttempts) {
					await Restart(e);
				}
			};

			discord.Connected += async () => {
				await config.BotOwner.SendMessageAsync($"Reconnected after {m_Attempts} attempts");

				m_Attempts = 0;
			};
		}

		private async Task Restart(Exception e) {
			string report = $"RoosterBot has been disconnected for more than twenty seconds. ";
			if (e == null) {
				report += "No exception is attached.";
			} else {
				report += $"The following exception is attached: \"{e.Message}\", stacktrace: {e.StackTrace}";
			}
			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await m_SNS.SendCriticalErrorNotificationAsync(report);

			Process.Start(new ProcessStartInfo(@"..\AppStart\AppStart.exe", "delay 20000"));
			Program.Instance.Shutdown();
		}
	}
}
