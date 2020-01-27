using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut, ItemTypeNameHandling = TypeNameHandling.Objects)]
	[DebuggerDisplay("{Activity.ScheduleCode} on {Start.ToString(\"yyyy-MM-dd\")} from {Start.ToString(\"hh:mm\")} to {End.ToString(\"hh:mm\")}")]
	public abstract class ScheduleRecord {
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; set; }
		public IReadOnlyList<StudentSetInfo> StudentSets { get; }
		public IReadOnlyList<StaffMemberInfo> StaffMember { get; }
		public IReadOnlyList<RoomInfo> Room { get; }
		public abstract bool ShouldCallNextCommand { get; }
		public BreakTime? Break { get; set; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<StaffMemberInfo> staffMember, IReadOnlyList<RoomInfo> room) {
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

		public abstract IEnumerable<AspectListItem> Present(ResourceService resources, CultureInfo culture);
		public abstract IReadOnlyList<string> PresentRow(ResourceService resources, CultureInfo culture);
	}
}
