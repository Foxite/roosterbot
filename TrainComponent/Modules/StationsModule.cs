using System;
using System.Threading.Tasks;
using Discord.Commands;
using PublicTransitComponent.Services;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Modules;

namespace PublicTransitComponent.Modules {
	[LogTag("PublicTransitModule")]
	public class StationsModule : RoosterModuleBase {
		public StationCodeService Stations { get; set; }

		[Command("stations", RunMode = RunMode.Async)]
		public async Task GetStationInfo([Remainder] string input) {
			StationMatchInfo[] matches = Stations.Lookup(input.ToLower(), 5);
			string response = "Gevonden stations zijn (beste match eerst):\n\n";

			int i = 1;
			foreach (StationMatchInfo match in matches) {
				response += $"{i}. {match.Station.DisplayName}";

				if (match.Station.Names.Length != 1) {
					string aka = " (ook bekend als: ";

					int count = 0;
					bool notFirst = false;
					for (int j = 1; j < match.Station.Names.Length; j++) {
						if (match.Station.Names[j] != match.Station.DisplayName.ToLower()) {
							if (notFirst) {
								aka += ", ";
							}
							aka += "\"" + match.Station.Names[j] + "\"";
							notFirst = true;
							count++;
						}
					}

					aka += ")";
					if (count != 0) {
						response += aka;
					}
				}

				response += $". Code: {match.Station.Code}\n";
				i++;
			}
			await ReplyAsync(response);
		}
	}
}