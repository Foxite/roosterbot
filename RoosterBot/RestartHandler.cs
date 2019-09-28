﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
		private Exception m_InitialException;

		public int MaxAttempts { get; }

		public RestartHandler(DiscordSocketClient discord, SNSService sns, int maxAttempts) {
			m_Attempts = 0;
			MaxAttempts = maxAttempts;
			m_SNS = sns;

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
				report += $"The initial exception is: {m_InitialException.ToString()}"; // TODO demystify after merge staging
			} else {
				report += "No initial exception was attached.\n\n";
			}

			if (e != null) {
				report += $"The last exception is: {e.ToString()}"; // This too
			} else {
				report += "No last exception was attached.\n\n";
			}

			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await m_SNS.SendCriticalErrorNotificationAsync(report);

			Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\AppStart\AppStart.exe"), "delay 20000"));
			Program.Instance.Shutdown();
		}
	}
}
