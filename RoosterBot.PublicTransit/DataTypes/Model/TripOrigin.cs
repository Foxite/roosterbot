using System;
using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record TripOrigin(
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
		string PlannedTrack, // string. perhaps there's a track "seven and a half" somewhere out there.

		[property: JsonProperty("actualTrack")]
		string ActualTrack
	) {
		public DateTimeOffset PlannedDateTimeOffset => new DateTimeOffset(PlannedDateTime, TimeSpan.FromMinutes(PlannedTimeZoneOffset));
		public DateTimeOffset ActualDateTimeOffset => new DateTimeOffset(ActualDateTime, TimeSpan.FromMinutes(ActualTimeZoneOffset));
	}
}
