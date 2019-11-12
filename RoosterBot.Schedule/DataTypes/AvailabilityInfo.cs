using System;

namespace RoosterBot.Schedule {
	public struct AvailabilityInfo {
		public DateTime StartOfAvailability { get; }
		public DateTime EndOfAvailability { get; }

		public AvailabilityInfo(DateTime startOfAvailability, DateTime endOfAvailability) {
			StartOfAvailability = startOfAvailability;
			EndOfAvailability = endOfAvailability;
		}

		public override bool Equals(object? obj) {
			return obj is AvailabilityInfo info
				&& StartOfAvailability == info.StartOfAvailability
				&& EndOfAvailability == info.EndOfAvailability;
		}

		public override int GetHashCode() => HashCode.Combine(StartOfAvailability, EndOfAvailability);

		public static bool operator ==(AvailabilityInfo left, AvailabilityInfo right) {
			return left.Equals(right);
		}

		public static bool operator !=(AvailabilityInfo left, AvailabilityInfo right) {
			return !(left == right);
		}
	}
}
