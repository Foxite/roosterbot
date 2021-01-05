using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StudentSetInfo : IdentifierInfo {
		public override bool AssignableToUser => true;

		// If you rename the parameter to anything else, you can't deserialize it anymore and everything will go to shit
		public StudentSetInfo(string scheduleCode) : base(scheduleCode) { }

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && (gluRecord.StudentSets.Contains(this) || gluRecord.StudentSets.Any(ssi => ssi.ScheduleCode == ScheduleCode[0..4]));
		}

		public override bool Equals(IdentifierInfo? other) => other is StudentSetInfo ssi && ssi.ScheduleCode.ToLower() == ScheduleCode.ToLower(); // hack to avoid reprocessing all our user's student sets (which are fully capitalized, while the schedules don't capitalize the subgroup letter)
		public override int GetHashCode() => ScheduleCode[0..4].GetHashCode();
	}
}
