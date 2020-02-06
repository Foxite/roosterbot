using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut, ItemTypeNameHandling = TypeNameHandling.Objects)]
	[DebuggerDisplay("{Activity.ScheduleCode} on {Start.ToString(\"yyyy-MM-dd\")} from {Start.ToString(\"hh:mm\")} to {End.ToString(\"hh:mm\")}")]
	public abstract class ScheduleRecord {
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; set; }
		public abstract bool ShouldCallNextCommand { get; }	
		public abstract IEnumerable<IdentifierInfo> InvolvedIdentifiers { get; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end) {
			Activity = activity;
			Start = start;
			End = end;
		}

		public abstract IEnumerable<AspectListItem> Present(ResourceService resources, CultureInfo culture);
		public abstract IReadOnlyList<string> PresentRow(ResourceService resources, CultureInfo culture);
	}
}
