using System.Linq;
using System.Reflection;

namespace ScheduleComponent.DataTypes {
	public class RoomInfo : IdentifierInfo {
		private static PropertyInfo s_RoomProperty = typeof(ScheduleRecord).GetProperty("StudentSets");
		public string Room { get; set; }

		public override bool Matches(ScheduleRecord record) {
			return record.Room.Contains(this);
		}

		public override string ScheduleField => "Room";
		public override string ScheduleCode => Room;
		public override string DisplayText => Room;
		public override PropertyInfo RelevantScheduleProperty => s_RoomProperty;
	}
}
