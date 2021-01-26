using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	public record Trip(
		[property: JsonProperty("plannedDurationInMinutes")]
		int PlannedDurationInMinutes,

		[property: JsonProperty("actualDurationInMinutes")]
		int ActualDurationInMinutes,

		[property: JsonProperty("transfers")]
		int Transfers,

		[property: JsonProperty("status")]
		TripStatus Status,

		[property: JsonProperty("legs")]
		IReadOnlyList<TripLeg> Legs,

		[property: JsonProperty("crowdForecast")]
		CrowdForecast CrowdForecast,

		[property: JsonProperty("punctuality")]
		float Punctuality
	) {
		public RoosterCommandResult Present() {
			// This approach works, technically, if your screen is wide enough (phone screen are universally not wide enough.
			// I'm going to try to think of a better way to present the data. If all else fails, I might make DiscordNet render wide tables to an image.
			string tableCaption = "";
			if (Status != TripStatus.Normal) {
				tableCaption = Status.ToString(); // TODO localize
			}

			string[][] cells = new string[Legs.Count + 1][];
			cells[0] = Legs[0].PresentRowHeading();

			for (int i = 0; i < Legs.Count; i++) {
				var leg = Legs[i];

				cells[i + 1] = leg.PresentRow();
			}
			return new TableResult(tableCaption, cells);
		}
	}
}
