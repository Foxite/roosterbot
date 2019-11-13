using Newtonsoft.Json;
using System;

namespace RoosterBot.Schedule {
	[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
	public class LastScheduleCommandInfo {
		public IdentifierInfo Identifier { get; set; }
		public DateTime? RecordEndTime { get; set; }

		public LastScheduleCommandInfo(IdentifierInfo identifier, DateTime? recordEndTime) {
			Identifier = identifier;
			RecordEndTime = recordEndTime;
		}
	}
}
