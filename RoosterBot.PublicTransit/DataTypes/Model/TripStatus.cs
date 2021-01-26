using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	[JsonConverter(typeof(CustomStringEnumConverter))]
	public enum TripStatus {
		[JsonProperty("CANCELLED")]
		Cancelled,

		[JsonProperty("CHANGE_NOT_POSSIBLE")]
        ChangeNotPossible,

		[JsonProperty("CHANGE_COULD_BE_POSSIBLE")]
        ChangeCouldBePossible,

		[JsonProperty("ALTERNATIVE_TRANSPORT")]
        AlternativeTransport,

		[JsonProperty("DISRUPTION")]
        Disruption,

		[JsonProperty("MAINTENANCE")]
        Maintenance,

		[JsonProperty("UNCERTAIN")]
        Uncertain,

		[JsonProperty("REPLACEMENT")]
        Replacement,

		[JsonProperty("ADDITIONAL")]
        Additional,

		[JsonProperty("SPECIAL")]
        Special,

		[JsonProperty("NORMAL")]
        Normal
	}
}
