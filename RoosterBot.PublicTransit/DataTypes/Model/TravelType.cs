using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	[JsonConverter(typeof(CustomStringEnumConverter))]
	public enum TravelType {
		[JsonProperty("PUBLIC_TRANSIT")]
		PublicTransit,

		[JsonProperty("WALK")]
        Walk,

		[JsonProperty("TRANSFER")]
        Transfer,

		[JsonProperty("BIKE")]
        Bike,

		[JsonProperty("CAR")]
        Car,

		[JsonProperty("KISS")]
        Kiss,

		[JsonProperty("TAXI")]
        Taxi,

		[JsonProperty("UNKNOWN")]
        Unknown
	}
}
