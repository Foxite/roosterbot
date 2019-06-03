using System;

namespace ScheduleComponent.DataTypes {
	public struct AvailabilityInfo {
		public DateTime StartOfAvailability { get; }
		public DateTime EndOfAvailability { get; }

		public AvailabilityInfo(DateTime startOfAvailability, DateTime endOfAvailability) {
			StartOfAvailability = startOfAvailability;
			EndOfAvailability = endOfAvailability;
		}
	}
}
