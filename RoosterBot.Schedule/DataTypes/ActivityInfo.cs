using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	public class ActivityInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText { get; }

		public ActivityInfo(string scheduleCode, string displayText) {
			ScheduleCode = scheduleCode;
			DisplayText = displayText;
		}

		public override bool Matches(ScheduleRecord info) {
			return info.Activity.ScheduleCode == ScheduleCode;
		}
	}
}
