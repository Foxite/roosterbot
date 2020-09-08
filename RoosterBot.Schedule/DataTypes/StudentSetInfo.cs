using System;
using System.Linq;

namespace RoosterBot.Schedule {
	public class StudentSetInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText => ScheduleCode;

		public StudentSetInfo(string scheduleCode) {
			ScheduleCode = scheduleCode;
		}

		public override bool Matches(ScheduleRecord record) {
			return record.StudentSets.Contains(this);
		}

		public override bool Equals(IdentifierInfo? other) => other != null && other.GetType() == GetType() && ((StudentSetInfo) other).ScheduleCode[0..3] == ScheduleCode[0..3];
		public override int GetHashCode() => ScheduleCode[0..3].GetHashCode();
	}
}
