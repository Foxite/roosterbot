using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleReader {
		public abstract Task<List<ScheduleRecord>> GetSchedule();
	}
}
