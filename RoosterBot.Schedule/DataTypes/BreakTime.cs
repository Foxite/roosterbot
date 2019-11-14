using System;

namespace RoosterBot.Schedule {
	public class BreakTime {
		public DateTime Start { get; }
		public DateTime End { get; }

		public BreakTime(DateTime start, DateTime end) {
			Start = start;
			End = end;
		}
	}
}
