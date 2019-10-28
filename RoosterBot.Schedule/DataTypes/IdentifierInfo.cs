using System;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfo : IEquatable<IdentifierInfo> {
		public abstract string ScheduleCode { get; }
		public abstract string DisplayText { get; }

		public abstract bool Matches(ScheduleRecord info);

		public override bool Equals(object other) {
			IdentifierInfo otherInfo = other as IdentifierInfo;
			if (other == null)
				return false;

			return Equals(otherInfo);
		}

		public bool Equals(IdentifierInfo other) {
			return other.GetType() == GetType() && other.ScheduleCode == ScheduleCode;
		}

		public override int GetHashCode() {
			var hashCode = 120372121;
			hashCode = hashCode * -1521134295 + ScheduleCode.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(IdentifierInfo lhs, IdentifierInfo rhs) {
			if (lhs is null != rhs is null)
				return false;
			if (lhs is null && rhs is null)
				return true;

			return lhs.Equals(rhs);
		}

		public static bool operator !=(IdentifierInfo lhs, IdentifierInfo rhs) {
			if (lhs is null != rhs is null)
				return true;
			if (lhs is null && rhs is null)
				return false;

			return !lhs.Equals(rhs);
		}

		public override string ToString() => DisplayText;
	}
}
