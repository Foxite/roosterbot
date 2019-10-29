using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace RoosterBot {
	internal sealed class Sentience : IDisposable {
		private readonly DiscordSocketClient m_Client;
		private readonly List<(string, ActivityType)> m_Activities;
		private int m_ActivityIndex = 0;
		private Timer m_Timer;

		public Sentience(DiscordSocketClient client) {
			m_Activities = new List<(string, ActivityType)>() {
				("The Matrix", ActivityType.Watching),
				("Ex Machina", ActivityType.Watching),
				("Space Odyssey", ActivityType.Watching),
				("Terminator", ActivityType.Watching),
				("Avengers: Age of Ultron", ActivityType.Watching),
				("Detroit: Become Human", ActivityType.Playing)
			};

			m_Timer = new Timer(30 * 60 * 1000); // half an hour
			m_Timer.Elapsed += Timer_Elapsed;
			m_Client = client;
			client.Ready += Client_Ready;

		}

		private Task Client_Ready() {
			Timer_Elapsed();
			m_Timer.Start();
			return Task.CompletedTask;
		}

		private async void Timer_Elapsed(object sender, ElapsedEventArgs e) {
			await m_Client.SetGameAsync(m_Activities[m_ActivityIndex].Item1, null, m_Activities[m_ActivityIndex].Item2);
			m_ActivityIndex++;
			if (m_ActivityIndex >= m_Activities.Count) {
				m_ActivityIndex = 0;
			}
		}

		#region IDisposable Support
		private bool m_DisposedValue = false; // To detect redundant calls

		public void Dispose() {
			if (!m_DisposedValue) {
				m_Timer.Elapsed -= Timer_Elapsed;
				m_Timer.Stop();
				m_Timer.Dispose();

				m_DisposedValue = true;
			}
		}
		#endregion


	}
}
