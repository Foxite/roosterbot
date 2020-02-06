using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StudentSetInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText => ScheduleCode;

		public StudentSetInfo(string scheduleCode) {
			ScheduleCode = scheduleCode;
		}

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && gluRecord.StudentSets.Contains(this);
		}
	}
}
