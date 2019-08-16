using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot.Services {
	public class StatisticsService {
		private ConfigService m_Config;
		private ConcurrentDictionary<string, int> m_IntegerStats;

		public StatisticsService(ConfigService config) {
			m_Config = config;

			m_IntegerStats = new ConcurrentDictionary<string, int>();
		}

		/// <summary>
		/// Publish statistics to ConfigService.LogChannel.
		/// </summary>
		/// <returns></returns>
		public async Task PublishStatistics() {
			if (m_Config.BotOwner != null) {
				await m_Config.BotOwner.SendMessageAsync(GenerateReport());
			} else {
				throw new NullReferenceException("The given ConfigService must have a LogChannel set.");
			}
		}

		public string GenerateReport() {
			string report = "24-hour statistic report.\n";
			if (!m_IntegerStats.IsEmpty) {
				report += "Integer stats:\n";
				foreach (KeyValuePair<string, int> kvp in m_IntegerStats) {
					report += $"{kvp.Key} : {kvp.Value}\n";
				}
			}
			return report;
		}

		public bool AddIntStatistic(string name, int initialValue = 0) {
			return m_IntegerStats.TryAdd(name, initialValue);
		}
	}
}
