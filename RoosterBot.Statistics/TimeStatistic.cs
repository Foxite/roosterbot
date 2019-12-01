using System;
using System.Collections.Concurrent;
using System.Timers;

namespace RoosterBot.Statistics {
	public class TimeStatistic : Statistic {
		private static Timer? s_CleanTimer;

		private readonly ConcurrentQueue<DateTime> m_Increments;

		public override int Count { get; }

		public TimeStatistic(Component nameKeyComponent, string nameKey) : base(nameKeyComponent, nameKey) {
			if (s_CleanTimer == null) {
				s_CleanTimer = new Timer(5 * 60) {
					Enabled = true,
					AutoReset = true
				};
			}

			m_Increments = new ConcurrentQueue<DateTime>();
			s_CleanTimer.Elapsed += CleanList;
		}

		private void CleanList(object sender, ElapsedEventArgs e) {
			while (m_Increments.TryPeek(out DateTime result) && (DateTime.Now - result) > TimeSpan.FromSeconds(30)) {
				m_Increments.TryDequeue(out result);
			}
		}

		public void Increment() {
			m_Increments.Enqueue(DateTime.Now);
		}
	}
}
