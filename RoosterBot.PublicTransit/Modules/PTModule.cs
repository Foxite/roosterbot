using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.PublicTransit {
	[RequireCulture("nl-NL", false)]
	[Name("OV")]
	public class PTModule : RoosterModule {
		public NSAPI NSAPI { get; set; } = null!;
		public StationInfoService Stations { get; set; } = null!;

		[Command("ov"), Description("Bereken een route van een station naar een andere (standaard vanaf Utrecht Vaartsche Rijn). Gebruik een komma tussen stations. Voorbeeld: `{0}ov amsterdam sloterdijk, utrecht centraal`")]
		public async Task<CommandResult> GetTrainRouteCommand([Name("vertrekstation, bestemming"), Count(1, 2), Remainder] StationInfo[] stops) {
			StationInfo stationFrom;
			StationInfo stationTo;
			if (stops.Length == 2) {
				stationFrom = stops[0];
				stationTo = stops[1];
			} else {
				stationFrom = Stations.DefaultDeparture;
				stationTo = stops[0];
			}

			IReadOnlyList<Trip>? journeys = await NSAPI.GetTravelRecommendation(stationFrom, stationTo, Context.Message.SentAt, false, null);

			throw new NotImplementedException();
			/*
			return new PaginatedResult(new BidirectionalListEnumerator<TableResult>(journeys.ListSelect(journey => {
				// TODO Move this code to Journey.Present()
				string tableCaption = "";
				if (journey.Status != JourneyStatus.OnSchedule) {
					tableCaption = JourneyStatusFunctions.HumanStringFromJStatus(journey.Status);
				}

				string[][] cells = new string[journey.Components.Count + 1][];
				cells[0] = new string[] { "Trein", "Vertrek om", "Vertrekspoor", "Aankomst", "Aankomst om", "Aankomstspoor", "Waarschuwing" };
				int recordIndex = 1;
				foreach (JourneyComponent component in journey.Components) {
					// TODO Move this code into JourneyComponent.PresentRow()
					cells[recordIndex] = new string[7];

					cells[recordIndex][0] = $"{component.Carrier} {component.TransportType}";

					cells[recordIndex][1] = component.Departure.Time.ToShortTimeString();
					if (component.Departure.DelayMinutes != 0) {
						cells[recordIndex][1] += $" (+{component.Departure.DelayMinutes}m)";
					}

					if (component.Departure.PlatformModified) {
						cells[recordIndex][2] = $"**{component.Departure.Platform}** (aangepast)";
					} else {
						cells[recordIndex][2] = component.Departure.Platform;
					}

					cells[recordIndex][3] = component.Arrival.Location;

					cells[recordIndex][4] = component.Arrival.Time.ToShortTimeString();
					if (component.Arrival.DelayMinutes != 0) {
						cells[recordIndex][4] += $" (+{component.Arrival.DelayMinutes}m)";
					}

					if (component.Arrival.PlatformModified) {
						cells[recordIndex][5] = $"**{component.Arrival.Platform}** (aangepast)";
					} else {
						cells[recordIndex][5] = component.Arrival.Platform;
					}

					if (component.Status != JourneyComponentStatus.OnSchedule) {
						cells[recordIndex][6] = JourneyStatusFunctions.HummanStringFromJCStatus(component.Status);
					} else {
						cells[recordIndex][6] = "";
					}

					recordIndex++;
				}
				return new TableResult(tableCaption, cells);
			})), $"Mogelijkheden van {stationFrom.Name} naar {stationTo.Name}:");
			*/
		}

		[Command("stations"), Description("Zoek een station op in de lijst.")]
		public CommandResult GetStationInfo([Remainder, Name("zoekterm")] string input) {
			string response = "Gevonden stations (beste match eerst):\n\n";

			IReadOnlyList<StationMatchInfo> matches = Stations.Lookup(input, 3);
			int i = 1;
			foreach (StationMatchInfo matchInfo in matches) {
				response += $"{i}. {matchInfo.Station.Name}";

				if (matchInfo.Station.Synonyms.Count != 1) {
					string aka = " (ook bekend als: ";

					int count = 0;
					bool notFirst = false;
					for (int j = 1; j < matchInfo.Station.Synonyms.Count; j++) {
						if (matchInfo.Station.Synonyms[j] != matchInfo.Station.Name.ToLower()) {
							if (notFirst) {
								aka += ", ";
							}
							aka += "\"" + matchInfo.Station.Synonyms[j] + "\"";
							notFirst = true;
							count++;
						}
					}

					aka += ")";
					if (count != 0) {
						response += aka;
					}
				}

				response += $". Code: {matchInfo.Station.Code}\n";
				i++;
			}
			return TextResult.Info(response);
		}
	}
}
