using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class RoomInfo : IdentifierInfo {
		public override bool AssignableToUser => false;

		public RoomInfo(string scheduleCode) : base(scheduleCode) { }

		public override bool Matches(ScheduleRecord record) {
			return record is GLUScheduleRecord gluRecord && gluRecord.Room.Contains(this);
		}
	}
}
