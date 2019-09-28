﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using RoosterBot.Services;

namespace RoosterBot {
	internal class DeadlockHandler {
		private SNSService m_SNS;
		private Timer m_Timer;
		private bool m_TimerRunning;

		internal int MaxDisconnectMillis { get; }

		internal DeadlockHandler(DiscordSocketClient discord, SNSService sns, int maxDisconnectMillis) {
			m_SNS = sns;
			MaxDisconnectMillis = maxDisconnectMillis;

			discord.Disconnected += (e) => {
				if (m_Timer != null) {
					m_Timer.Dispose();
					m_Timer = null;
				}
				m_Timer = new Timer(TimerCallback, e, MaxDisconnectMillis, -1);
				m_TimerRunning = true;
				return Task.CompletedTask;
			};

			discord.Connected += () => {
				m_TimerRunning = false;
				if (m_Timer != null) {
					m_Timer.Dispose();
					m_Timer = null;
				}
				return Task.CompletedTask;
			};
		}

		private async void TimerCallback(object state) {
			if (m_TimerRunning) { // Prevent race condition
				await Restart(state as Exception);
			}
		}

		private async Task Restart(Exception e) {
			string report = $"RoosterBot has failed to reconnect after {MaxDisconnectMillis / 1000} seconds.\n\n";

			if (e != null) {
				report += $"The exception is: {e.ToString()}";
			} else {
				report += "No exception was attached.\n\n";
			}

			report += "\n\nThe bot will attempt to restart in 20 seconds.";
			await m_SNS.SendCriticalErrorNotificationAsync(report);

			Process.Start(new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\AppStart\AppStart.exe"), "delay 20000"));
			Program.Instance.Shutdown();
		}
	}
}
