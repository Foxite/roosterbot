using System;

namespace RoosterBot.Schedule {
	public struct AvailabilityInfo {
		public DateTime StartOfAvailability { get; }
		public DateTime EndOfAvailability { get; }

		public AvailabilityInfo(DateTime startOfAvailability, DateTime endOfAvailability) {
			StartOfAvailability = startOfAvailability;
			EndOfAvailability = endOfAvailability;
		}
	}
}
