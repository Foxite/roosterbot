using System;

namespace RoosterBot {
	public static partial class Constants {
		public static readonly DateTime FirstReleaseDate = new DateTime(2018, 10, 1);
		public static readonly int DaysSinceV1 = (int) (CurrentReleaseDate - FirstReleaseDate).TotalDays;
		public static readonly string DeployVersion = DaysSinceV1.ToString() + "." + ReleasesToday;

		public static readonly Version RoosterBotVersion = new Version(2, 0, 0);
		public static string VersionString => RoosterBotVersion.ToString();
	}
}
