using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	[JsonConverter(typeof(CustomStringEnumConverter))]
	public enum ExitSide {
		[JsonProperty("LEFT")]
		Left,

		[JsonProperty("RIGHT")]
		Right,

		[JsonProperty("UNKNOWN")]
		Unknown
	}
}
