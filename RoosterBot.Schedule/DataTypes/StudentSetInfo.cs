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
	}
}
