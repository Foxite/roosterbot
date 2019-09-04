using System;
using System.Collections.Generic;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfo : IEquatable<IdentifierInfo> {
		public abstract string ScheduleField { get; }
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
			return other.ScheduleCode == ScheduleCode
				&& other.ScheduleField == ScheduleField;
		}

		public override int GetHashCode() {
			return 53717137 + EqualityComparer<string>.Default.GetHashCode(ScheduleCode);
		}

		public static bool operator ==(IdentifierInfo lhs, IdentifierInfo rhs) {
			if (lhs is null != rhs is null)
				return false;
			if (lhs is null && rhs is null)
				return true;

			return lhs.ScheduleCode == rhs.ScheduleCode
				&& lhs.ScheduleField == rhs.ScheduleField;
		}

		public static bool operator !=(IdentifierInfo lhs, IdentifierInfo rhs) {
			if (lhs is null != rhs is null)
				return true;
			if (lhs is null && rhs is null)
				return false;

			return lhs.ScheduleCode != rhs.ScheduleCode
				|| lhs.ScheduleField != rhs.ScheduleField;
		}

		public override string ToString() => DisplayText;
	}
}
