using System.Linq;

namespace RoosterBot.Schedule {
	public class StudentSetInfo : IdentifierInfo {
		public string ClassName { get; }

		public StudentSetInfo(string className) {
			ClassName = className;
		}

		public override bool Matches(ScheduleRecord record) {
			return record.StudentSets.Contains(this);
		}

		public override string ScheduleCode => ClassName;
		public override string DisplayText => ClassName;
	}
}
