using System.Linq;
using System.Reflection;

namespace RoosterBot.Schedule {
	public class StudentSetInfo : IdentifierInfo {
		private static PropertyInfo s_StudentSetsProperty = typeof(ScheduleRecord).GetProperty("StudentSets");

		public string ClassName { get; set; }

		public override bool Matches(ScheduleRecord record) {
			return record.StudentSets.Contains(this);
		}

		public override string ScheduleField => "StudentSets";
		public override string ScheduleCode => ClassName;
		public override string DisplayText => ClassName;
		public override PropertyInfo RelevantScheduleProperty => s_StudentSetsProperty;
	}
}
