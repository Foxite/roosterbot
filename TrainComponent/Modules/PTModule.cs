using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Modules;
using PublicTransitComponent.Services;
using PublicTransitComponent.DataTypes;
using RoosterBot.Attributes;
using RoosterBot;

namespace PublicTransitComponent.Modules {
	[LogTag("PublicTransitModule")]
	public class PTModule : RoosterModuleBase {
		public NSAPI NSAPI { get; set; }
		public StationCodeService Stations { get; set; }

		[Command("ov", RunMode = RunMode.Async)]
		public async Task GetTrainRouteCommand([Remainder] string van_en_naar) {
			string[] stops = van_en_naar.Split(',');
			if (stops.Length < 1 || stops.Length > 2) {
				await MinorError("Ik moet ten minste 1 station hebben, en maximaal 2.");
				return;
			}

			StationInfo stationFrom;
			StationInfo stationTo;
			if (stops.Length == 2) {
				stationFrom = Stations.Lookup(stops[0]);
				stationTo = Stations.Lookup(stops[1]);
			} else {
				stationFrom = Stations.DefaultDeparture;
				stationTo = Stations.Lookup(stops[0]);
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
				ReplyDeferred(pretext + "\n" + Util.FormatTextTable(cells, true));
			}
		}
	}
}
