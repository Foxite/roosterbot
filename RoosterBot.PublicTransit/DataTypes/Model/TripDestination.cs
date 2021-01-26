using System;
using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record TripDestination(
		[property: JsonProperty("name")]
		string Name,

		[property: JsonProperty("plannedTimeZoneOffset")]
		int PlannedTimeZoneOffset,

		[property: JsonProperty("plannedDateTime")]
		DateTime PlannedDateTime,

		[property: JsonProperty("actualTimeZoneOffset")]
		int ActualTimeZoneOffset,

		[property: JsonProperty("actualDateTime")]
		DateTime ActualDateTime,

		[property: JsonProperty("plannedTrack")]
		string PlannedTrack,

		[property: JsonProperty("actualTrack")]
		string ActualTrack,

		[property: JsonProperty("exitSide")]
		ExitSide ExitSide // TODO enum
	) {
		public DateTimeOffset PlannedDateTimeOffset => new DateTimeOffset(PlannedDateTime, TimeSpan.FromMinutes(PlannedTimeZoneOffset));
		public DateTimeOffset ActualDateTimeOffset => new DateTimeOffset(ActualDateTime, TimeSpan.FromMinutes(ActualTimeZoneOffset));
	}
}
