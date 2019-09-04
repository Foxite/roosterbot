using System.Linq;

namespace RoosterBot.Schedule {
	public class StudentSetInfo : IdentifierInfo {
		public string ClassName { get; set; }

		public override bool Matches(ScheduleRecord record) {
			return record.StudentSets.Contains(this);
		}

		public override string ScheduleField => "StudentSets";
		public override string ScheduleCode => ClassName;
		public override string DisplayText => ClassName;
	}
}
