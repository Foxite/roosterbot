using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record TripsResponse(
		//string source,

		[property: JsonProperty("trips")]
		IReadOnlyList<Trip> Trips,

		[property: JsonProperty("scrollRequestBackwardContext")]
		string BackwardContext,

		[property: JsonProperty("scrollRequestForwardContext")]
		string ForwardContext

	);
}
