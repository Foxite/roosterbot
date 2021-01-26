using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record TripLeg(
		[property: JsonProperty("name")]
		string Name,

		[property: JsonProperty("travelType")]
		TravelType TravelType,

		[property: JsonProperty("direction")]
		string Direction, // Could be StationInfo

		[property: JsonProperty("cancelled")]
		bool Cancelled,

		[property: JsonProperty("changePossible")]
		bool ChangePossible,

		[property: JsonProperty("alternativeTransport")]
		bool AlternativeTransport,

		[property: JsonProperty("origin")]
		TripOrigin Origin,

		[property: JsonProperty("destination")]
		TripDestination Destination,

		[property: JsonProperty("product")]
		Transport Product,

		[property: JsonProperty("crowdForecast")]
		CrowdForecast CrowdForecast,

		[property: JsonProperty("punctuality")]
		float Punctuality,

		[property: JsonProperty("crossPlatformTransfer")]
		bool CrossPlatformTransfer,

		[property: JsonProperty("shorterStock")]
		bool ShorterStock,

		[property: JsonProperty("reachable")]
		bool Reachable,

		[property: JsonProperty("plannedDurationInMinutes")]
		int PlannedDurationInMinutes
	) {
		public string[] PresentRowHeading() => new string[] { "Trein", "Richting", "Vertrek", "Aankomst" };

		public string[] PresentRow() {
			var ret = new string[5];

			ret[0] = Product.DisplayName;
			ret[1] = Direction;

			ret[2] = Origin.ActualDateTimeOffset.ToString("t"); // TODO apply user timezone
			if (Origin.ActualDateTimeOffset != Origin.PlannedDateTimeOffset) {
				ret[2] += $" (+{(int) (Origin.ActualDateTimeOffset - Origin.PlannedDateTimeOffset).TotalMinutes}m)";
			}

			if (Origin.PlannedTrack != Origin.ActualTrack) {
				ret[2] += $", 🚆 {Origin.ActualTrack} !";
			} else {
				ret[2] += $", 🚆 {Origin.PlannedTrack}";
			}

			ret[3] = Destination.Name + " " + Destination.ActualDateTimeOffset.ToString("t");
			if (Destination.ActualDateTimeOffset != Origin.PlannedDateTimeOffset) {
				ret[3] += $" (+{(int) (Destination.ActualDateTimeOffset - Destination.PlannedDateTimeOffset).TotalMinutes}m)";
			}
			
			if (Destination.PlannedTrack != Destination.ActualTrack) {
				ret[3] += $", 🚆 {Destination.ActualTrack} !";
			} else {
				ret[3] += $", 🚆 {Destination.PlannedTrack}";
			}
			return ret;
		}
	}
}
