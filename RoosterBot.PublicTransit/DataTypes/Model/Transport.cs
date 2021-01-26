using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record Transport(
		[property: JsonProperty("displayName")]
		string DisplayName
	);
}
