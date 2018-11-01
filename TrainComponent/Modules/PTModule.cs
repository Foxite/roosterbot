using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Modules;
using PublicTransitComponent.Services;
using PublicTransitComponent.DataTypes;

namespace PublicTransitComponent.Modules {
	public class PTModule : EditableCmdModuleBase {
		public NSAPI NSAPI { get; set; }
		
		public PTModule() {
			LogTag = "PTM";
		}

		[Command("ov", RunMode = RunMode.Async)]
		public async Task GetTrainRouteCommand(string from, string to) {
			Journey[] journeys = await NSAPI.GetTravelRecommendation(2, from, to);
			string response = "Mogelijkheden:\n";

			for (int i = 0; i < journeys.Length; i++) {
				response += "`" + (i + 1) + ". `";
				bool isNotFirstLine = false;
				foreach (JourneyComponent component in journeys[i].Components) {
					if (isNotFirstLine) {
						response += "`   ` ";
					}
					response += $"{component.Carrier} {component.TransportType}";
					JourneyStop departure = component.Departure;
					JourneyStop arrival = component.Arrival;
					response += $" om {departure.Time.ToShortTimeString()} naar {arrival.Location} (spoor {departure.Platform}{(departure.PlatformModified ? " - gewijzigd" : "")})";
					if (departure.DelayMinutes != 0) {
						response += $" - {departure.DelayMinutes} minuten vertraagd";
					}
					response += "\n";

					isNotFirstLine = true;
				}
				response += "\n";
			}
			await ReplyAsync(response);
		}
	}
}
