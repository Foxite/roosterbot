using System.Collections.Generic;

namespace RoosterBot.Schedule {
	public abstract class ScheduleReader {
		public abstract IReadOnlyList<ScheduleRecord> GetSchedule();
	}
}
