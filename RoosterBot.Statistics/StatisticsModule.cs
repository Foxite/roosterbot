﻿using System.Linq;
using Qmmands;

namespace RoosterBot.Statistics {
	public class StatisticsModule : RoosterModule {
		public StatisticsService Statistics { get; set; } = null!;

		[Command("statistics", "stats", "statistic", "stat")]
		public CommandResult GetAllStatistics() {
			return new TextResult(null, string.Join('\n',
				Statistics.GetAllStatistics().Select(stat => Resources.ResolveString(Culture, stat.NameKeyComponent, stat.NameKey) + ": " + stat.Count)));
		}
	}
}
