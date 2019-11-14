using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut, ItemTypeNameHandling = TypeNameHandling.Objects)]
	public abstract class ScheduleRecord {
		// TODO (review) Do all these properties need to be in Schedule component? It's better to move everything into GLUScheduleRecord, and all identifiers types into Schedule.GLU
		// An abstract PresentRowAsync will be necessary for this
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; set; }
		public IReadOnlyList<StudentSetInfo> StudentSets { get; }
		public IReadOnlyList<TeacherInfo> StaffMember { get; }
		public IReadOnlyList<RoomInfo> Room { get; }
		public abstract bool ShouldCallNextCommand { get; }
		public BreakTime? Break { get; set; }
		
		protected TimeZoneInfo Timezone { get; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room, TimeZoneInfo timezone) {
			Activity = activity;
			Start = start;
			End = end;
			StudentSets = studentSets;
			StaffMember = staffMember;
			Room = room;
			Timezone = timezone;
		}

		[JsonIgnore] public TimeSpan Duration => End - Start;
		[JsonIgnore] public string StudentSetsString => string.Join(", ", StudentSets.Select(info => info.ScheduleCode));
		[JsonIgnore] public string StaffMemberString => string.Join(", ", StaffMember.Select(info => info.DisplayText));
		[JsonIgnore] public string RoomString => string.Join(", ", Room.Select(info => info.ScheduleCode));

		public override string ToString() {
			return $"{StudentSetsString}: {Activity.ScheduleCode} in {RoomString} from {Start.ToString()} (for {(int) Duration.TotalHours}:{Duration.Minutes}) (with " +
				$"{(Break == null ? "no break" : ("a break from " + Break.Start.ToString() + " to " + Break.End.ToString()))}) to {End.ToString()} by {StaffMember}";
		}

		/// <summary>
		/// Convert this instance to a string that can be sent to Discord.
		/// </summary>
		// TODO (review) Should this really be async? The instance is supposed to have all the data it needs, and shouldn't have any service reference to get data from
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
