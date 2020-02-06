namespace RoosterBot.Schedule {
	public class ActivityInfo : IdentifierInfo {
		public override string DisplayText { get; }
		public override bool AssignableToUser => false;

		public ActivityInfo(string scheduleCode, string displayText) : base(scheduleCode) {
			DisplayText = displayText;
		}

		public override bool Matches(ScheduleRecord info) {
			return info.Activity.ScheduleCode == ScheduleCode;
		}
	}
}
