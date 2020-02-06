using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class RoomInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText => ScheduleCode;
		public override bool AssignableToUser => true;

		public RoomInfo(string scheduleCode) {
			ScheduleCode = scheduleCode;
		}

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && gluRecord.Room.Contains(this);
		}
	}
}
