using System.Linq;

namespace RoosterBot.Schedule {
	public class RoomInfo : IdentifierInfo {
		public string Room { get; }

		public RoomInfo(string room) {
			Room = room;
		}

		public override bool Matches(ScheduleRecord record) {
			return record.Room.Contains(this);
		}

		public override string ScheduleCode => Room;
		public override string DisplayText => Room;
	}
}
