using System;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfo : IEquatable<IdentifierInfo> {
		public abstract string ScheduleCode { get; }
		public abstract string DisplayText { get; }

		public abstract bool Matches(ScheduleRecord info);

		public override bool Equals(object? other) {
			IdentifierInfo? otherInfo = other as IdentifierInfo;
			if (other == null)
				return false;

			return Equals(otherInfo);
		}

		public bool Equals(IdentifierInfo? other) {
			return other != null && other.GetType() == GetType() && other.ScheduleCode == ScheduleCode;
		}

		public override int GetHashCode() {
			var hashCode = 120372121;
			hashCode = hashCode * -1521134295 + ScheduleCode.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(IdentifierInfo? lhs, IdentifierInfo? rhs) {
			if (lhs is null != rhs is null)
				return false;
			if (lhs is null && rhs is null)
				return true;

			// There's no way for lhs to be null at this point, but the compiler doesn't seem to get that.
			// The only way to get to this point is if both lhs and rhs are not null, any other combination of null-ness will result in a return before this point.
			// Fortunately you can avoid the warning with a !.
			return lhs!.Equals(rhs);
		}

		public static bool operator !=(IdentifierInfo? lhs, IdentifierInfo? rhs) {
			if (lhs is null != rhs is null)
				return true;
			if (lhs is null && rhs is null)
				return false;

			// See comments in operator ==.
			return !lhs!.Equals(rhs);
		}

		public override string ToString() => DisplayText;
	}
}
