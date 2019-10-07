using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.PublicTransit {
	[RequireCulture("nl-NL", false)]
	[Name("OV")]
	public class PTModule : RoosterModuleBase {
		public NSAPI NSAPI { get; set; }
		public StationInfoService Stations { get; set; }

		// TODO this currently does not work, because the documentation regarding "params" in command args does not match what actually happens
		// I'm waiting for a response to the issue I created ( https://github.com/discord-net/Discord.Net/issues/1394 ), until then this is on hold.
		[Command("ov", RunMode = RunMode.Async), Summary("Bereken een route van een station naar een andere (standaard vanaf Utrecht Vaartsche Rijn). Gebruik een komma tussen stations. Voorbeeld: `!ov amsterdam sloterdijk, utrecht centraal`")]
		public async Task GetTrainRouteCommand([Name("vertrekstation, bestemming")] params StationInfo[] stops) {
			if (stops.Length < 1 || stops.Length > 2) {
				await MinorError("Ik moet ten minste 1 station hebben, en maximaal 2.");
				return;
			}

			StationInfo stationFrom;
			StationInfo stationTo;
			if (stops.Length == 2) {
				stationFrom = stops[0];
				stationTo = stops[1];
			} else {
				stationFrom = Stations.DefaultDeparture;
				stationTo = stops[0];
			}

			await ReplyAsync($"Mogelijkheden van {stationFrom.DisplayName} naar {stationTo.DisplayName} opzoeken...");

			Journey[] journeys = await NSAPI.GetTravelRecommendation(2, stationFrom.Code, stationTo.Code);

			ReplyDeferred($"{Context.User.Mention} Mogelijkheden: (elke optie is in een aparte tabel, en elke rij is één overstap)");

			foreach (Journey journey in journeys) {
				string pretext = "";
				if (journey.Status != JourneyStatus.OnSchedule) {
					pretext = JourneyStatusFunctions.HumanStringFromJStatus(journey.Status);
				}

				string[][] cells = new string[journey.Components.Count + 1][];
				cells[0] = new string[] { "Trein", "Vertrek om", "Vertrekspoor", "Aankomst", "Aankomst om", "Aankomstspoor", "Waarschuwing" };
				int recordIndex = 1;
				foreach (JourneyComponent component in journey.Components) {
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
				ReplyDeferred(pretext + "\n" + Util.FormatTextTable(cells));
			}
		}

		[Command("stations"), Summary("Zoek een station op in de lijst.")]
		public async Task GetStationInfo([Remainder, Name("zoekterm")] string input) {
			string response = "Gevonden stations zijn (beste match eerst):\n\n";

			StationMatchInfo[] matches = Stations.Lookup(input, 3);
			int i = 1;
			foreach (StationMatchInfo matchInfo in matches) {
				response += $"{i}. {matchInfo.Station.DisplayName}";

				if (matchInfo.Station.Synonyms.Count != 1) {
					string aka = " (ook bekend als: ";

					int count = 0;
					bool notFirst = false;
					for (int j = 1; j < matchInfo.Station.Synonyms.Count; j++) {
						if (matchInfo.Station.Synonyms[j] != matchInfo.Station.DisplayName.ToLower()) {
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
			await ReplyAsync(response);
		}
	}
}
