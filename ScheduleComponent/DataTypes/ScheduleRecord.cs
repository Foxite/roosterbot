using System;
using System.Linq;

namespace ScheduleComponent.DataTypes {
	public class ScheduleRecord {
		public string Activity { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public StudentSetInfo[] StudentSets { get; set; }
		public TeacherInfo[] StaffMember { get; set; }
		public RoomInfo[] Room { get; set; }
		public DateTime? BreakStart { get; set; }
		public DateTime? BreakEnd { get; set; }

		public TimeSpan Duration => End - Start;
		public string StudentSetsString => string.Join(", ", StudentSets.Select(info => info.ClassName));
		public string StaffMemberString => string.Join(", ", StaffMember.Select(info => info.DisplayText));
		public string RoomString => string.Join(", ", Room.Select(info => info.Room));

		public override string ToString() {
			return $"{StudentSetsString}: {Activity} in {RoomString} from {Start.ToString()} (for {(int) Duration.TotalHours}:{Duration.Minutes}) (with " +
				$"{(BreakStart.HasValue ? "no break" : ("a break from " + BreakStart.Value.ToString() + " to " + BreakEnd.Value.ToString()))}) to {End.ToString()} by {StaffMember}";
		}
	}
}
