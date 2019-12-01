using System.Collections.Generic;

namespace RoosterBot.Statistics {
	public sealed class StatisticsService {
		private readonly ResourceService m_Resources;
		private readonly List<Statistic> m_Stats;

		internal StatisticsService(ResourceService resources) {
			m_Stats = new List<Statistic>();
			m_Resources = resources;
		}

		public void AddStatistic(Statistic stat) {
			m_Stats.Add(stat);
		}

		public ICollection<Statistic> GetAllStatistics() => m_Stats.AsReadOnly();
	}
}
