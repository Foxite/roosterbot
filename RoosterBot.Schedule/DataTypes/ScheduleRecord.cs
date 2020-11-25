﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut, ItemTypeNameHandling = TypeNameHandling.Objects)]
	[DebuggerDisplay("{Activity.ScheduleCode} on {Start.ToString(\"yyyy-MM-dd\")} from {Start.ToString(\"hh:mm\")} to {End.ToString(\"hh:mm\")}")]
	public abstract record ScheduleRecord {
		public ActivityInfo Activity { get; }
		public DateTime Start { get; }
		public DateTime End { get; }

		/// <summary>
		/// Used in optimizing <see cref="MemoryScheduleProvider"/>. If you don't use that provider, this will be unused.
		/// </summary>
		public abstract IEnumerable<IdentifierInfo> InvolvedIdentifiers { get; }

		protected ScheduleRecord(ActivityInfo activity, DateTime start, DateTime end) {
			Activity = activity;
			Start = start;
			End = end;
		}

		public abstract IEnumerable<AspectListItem> Present(RoosterCommandContext context);

		/// <summary>
		/// Returns a list of strings used as the table heading for the rows returned by <see cref="PresentRow(RoosterCommandContext)"/>.
		/// 
		/// When implementing this function, keep in mind that multiple records WILL be used in a single table, which means that <see cref="PresentRow(RoosterCommandContext)"/>
		///  and this function should always return the same format, regardless of context. In particular, this function should be considered as a static function, as the output should
		///  not involve reading instance variables of this class.
		/// </summary>
		public abstract IReadOnlyList<string> PresentRowHeadings(RoosterCommandContext context);

		/// <summary>
		/// Return a list of strings used as cells when presenting this record as a row in a table. <see cref="PresentRowHeadings(RoosterCommandContext)"/> provides the heading
		///  cells for this function.
		/// 
		/// When implementing this function, keep in mind that multiple records WILL be used in a single table, which means that <see cref="PresentRowHeadings(RoosterCommandContext)"/>
		///  and this function should always return the same format, regardless of context. Empty or non-applicable cells should be filled in with an empty string, as skipping them will
		///  result in breaks or errors when formatting the table.
		/// </summary>
		public abstract IReadOnlyList<string> PresentRow(RoosterCommandContext context);
	}
}
