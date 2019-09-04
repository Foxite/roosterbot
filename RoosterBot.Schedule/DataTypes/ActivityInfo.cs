namespace RoosterBot.Schedule {
	public class ActivityInfo : IdentifierInfo {
		public ActivityInfo(string scheduleCode, string displayText) {
			ScheduleCode = scheduleCode;
			DisplayText = displayText;
		}

		public override string ScheduleField => "Activity";
		public override string DisplayText { get; }
		public override string ScheduleCode { get; }

		public override bool Matches(ScheduleRecord info) {
			return info.Activity.ScheduleCode == ScheduleCode;
		}
	}
}
