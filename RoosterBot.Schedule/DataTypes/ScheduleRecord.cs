using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleRecord {
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; set; }
		public IReadOnlyList<StudentSetInfo> StudentSets { get; }
		public IReadOnlyList<TeacherInfo> StaffMember { get; }
		public IReadOnlyList<RoomInfo> Room { get; }
		public BreakTime? Break { get; set; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room) {
			Activity = activity;
			Start = start;
			End = end;
			StudentSets = studentSets;
			StaffMember = staffMember;
			Room = room;
		}

		public TimeSpan Duration => End - Start;
		public string StudentSetsString => string.Join(", ", StudentSets.Select(info => info.ClassName));
		public string StaffMemberString => string.Join(", ", StaffMember.Select(info => info.DisplayText));
		public string RoomString => string.Join(", ", Room.Select(info => info.Room));
		public abstract bool ShouldCallNextCommand { get; }

		public override string ToString() {
			return $"{StudentSetsString}: {Activity.ScheduleCode} in {RoomString} from {Start.ToString()} (for {(int) Duration.TotalHours}:{Duration.Minutes}) (with " +
				$"{(Break == null ? "no break" : ("a break from " + Break.Start.ToString() + " to " + Break.End.ToString()))}) to {End.ToString()} by {StaffMember}";
		}

		/// <summary>
		/// Convert this instance to a string that can be sent to Discord.
		/// </summary>
		public abstract Task<string> PresentAsync(IdentifierInfo relevantIdentifier);
	}

	public class BreakTime {
		public DateTime Start { get; }
		public DateTime End { get; }

		public BreakTime(DateTime start, DateTime end) {
			Start = start;
			End = end;
		}
	}
}
