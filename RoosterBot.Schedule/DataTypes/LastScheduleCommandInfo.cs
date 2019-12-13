using System;
using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
	public class LastScheduleCommandInfo {
		public IdentifierInfo Identifier { get; set; }
		public DateTime? RecordEndTime { get; set; }
		public ScheduleResultKind Kind { get; set; }

		public LastScheduleCommandInfo(IdentifierInfo identifier, DateTime? recordEndTime, ScheduleResultKind kind) {
			Identifier = identifier;
			RecordEndTime = recordEndTime;
			Kind = kind;
		}
	}

	public enum ScheduleResultKind {
		Single,
		Day,
		Week
	}
}
