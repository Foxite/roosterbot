using ScheduleComponent.DataTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScheduleComponent.Services {
	public abstract class ScheduleReaderBase {
		public abstract Task<List<ScheduleRecord>> GetSchedule(string name);
	}
}
