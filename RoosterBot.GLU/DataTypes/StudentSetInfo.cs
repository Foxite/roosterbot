using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StudentSetInfo : IdentifierInfo {
		public override bool AssignableToUser => true;

		public StudentSetInfo(string code) : base(code) { }

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && gluRecord.StudentSets.Contains(this);
		}
	}
}
