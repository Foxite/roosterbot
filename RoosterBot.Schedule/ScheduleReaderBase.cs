using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleReaderBase {
		public abstract Task<List<ScheduleRecord>> GetSchedule();
	}
}
