using Newtonsoft.Json;
using System.Linq;

namespace RoosterBot.Schedule {
	public class RoomInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText => ScheduleCode;

		public RoomInfo(string scheduleCode) {
			ScheduleCode = scheduleCode;
		}

		public override bool Matches(ScheduleRecord record) {
			return record.Room.Contains(this);
		}
	}
}
