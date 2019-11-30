using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut, ItemTypeNameHandling = TypeNameHandling.Objects)]
	public abstract class ScheduleRecord {
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; set; }
		public IReadOnlyList<StudentSetInfo> StudentSets { get; }
		public IReadOnlyList<TeacherInfo> StaffMember { get; }
		public IReadOnlyList<RoomInfo> Room { get; }
		public abstract bool ShouldCallNextCommand { get; }
		public BreakTime? Break { get; set; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room) {
			Activity = activity;
			Start = start;
			End = end;
			StudentSets = studentSets;
			StaffMember = staffMember;
			Room = room;
		}

		[JsonIgnore] public TimeSpan Duration => End - Start;
		[JsonIgnore] public string StudentSetsString => string.Join(", ", StudentSets.Select(info => info.ScheduleCode));
		[JsonIgnore] public string StaffMemberString => string.Join(", ", StaffMember.Select(info => info.DisplayText));
		[JsonIgnore] public string RoomString => string.Join(", ", Room.Select(info => info.ScheduleCode));

		public override string ToString() {
			return $"{StudentSetsString}: {Activity.ScheduleCode} in {RoomString} from {Start.ToString()} (for {(int) Duration.TotalHours}:{Duration.Minutes}) (with " +
				$"{(Break == null ? "no break" : ("a break from " + Break.Start.ToString() + " to " + Break.End.ToString()))}) to {End.ToString()} by {StaffMember}";
		}

		public abstract IEnumerable<AspectListItem> Present(CultureInfo culture);
		public abstract IReadOnlyList<string> PresentRow(CultureInfo culture);
	}
}
