using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleReader {
		#nullable enable
		public abstract Task<List<ScheduleRecord>> GetSchedule();
	}
}
