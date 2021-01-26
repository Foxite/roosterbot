using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	[JsonConverter(typeof(CustomStringEnumConverter))]
	public enum CrowdForecast {
		[JsonProperty("UNKNOWN")]
		Unknown,

		[JsonProperty("LOW")]
		Low,

		[JsonProperty("MEDIUM")]
		Medium,

		[JsonProperty("HIGH")]
		High
	}
}
