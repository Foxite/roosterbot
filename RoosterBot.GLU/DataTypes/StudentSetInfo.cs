using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StudentSetInfo : IdentifierInfo {
		public override bool AssignableToUser => true;

		// If you rename the parameter to anything else, you can't deserialize it anymore and everything will go to shit
		public StudentSetInfo(string scheduleCode) : base(scheduleCode) { }

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && gluRecord.StudentSets.Contains(this);
		}

		public override bool Equals(IdentifierInfo? other) => other != null && other.GetType() == GetType() && ((StudentSetInfo) other).ScheduleCode[0..3] == ScheduleCode[0..3];
		public override int GetHashCode() => ScheduleCode[0..3].GetHashCode();
	}
}
