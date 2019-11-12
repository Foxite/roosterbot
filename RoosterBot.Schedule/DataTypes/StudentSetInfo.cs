using Newtonsoft.Json;
using System.Linq;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptIn)]
	public class StudentSetInfo : IdentifierInfo {
		[JsonProperty] public override string ScheduleCode { get; }
		public override string DisplayText	=> ScheduleCode;

		public StudentSetInfo(string scheduleCode) {
			ScheduleCode = scheduleCode;
		}

		public override bool Matches(ScheduleRecord record) {
			return record.StudentSets.Contains(this);
		}
	}
}
