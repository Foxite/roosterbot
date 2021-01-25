using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfoService {
		private readonly List<StationInfo> m_Stations;

		public StationInfo DefaultDeparture { get; private set; }

		public StationInfoService(string stationFilePath, string defaultDepartureCode) {
			Logger.Info(PublicTransitComponent.LogTag, "Loading stations file");

			m_Stations = new List<StationInfo>();

			// TODO read data from (probably json) file
			// Assign DefaultDeparture to the first station you find in the file with the given code
			
			if (DefaultDeparture == null) {
				throw new InvalidOperationException("No station matching the default departure code was found in the stations file.");
			}
			Logger.Info(PublicTransitComponent.LogTag, "Finished loading stations file");
		}

		public StationInfo? GetByCode(string code) {
			return m_Stations.Find(info =>  info.Code == code);
		}

		public StationMatchInfo[] Lookup(string input, int count) {
			string inputLower = input.ToLower();
			var matches = new LinkedList<StationMatchInfo>();
			
			void insertMatch(StationMatchInfo match) {
				LinkedListNode<StationMatchInfo>? insertAfter = null;
				foreach (LinkedListNode<StationMatchInfo> current in matches.GetNodes()) {
					if (current.Value.Score < match.Score) {
						insertAfter = current;
					} else {
						break;
					}
				}

				if (insertAfter == null) {
					matches.AddFirst(match);
				} else {
					matches.AddAfter(insertAfter, match);
				}
				if (matches.Count > count) {
					matches.RemoveLast();
				}
			}

			foreach (StationInfo v in m_Stations) {
				insertMatch(new StationMatchInfo(v, v.Match(inputLower)));
			}

			if (count == 1) {
				Logger.Debug(PublicTransitComponent.LogTag, $"Asked for 1 for `{input}`: result is {matches.First!.Value.Station.DisplayName} with {matches.First.Value.Score}");
			} else if (count == 0) {
				Logger.Debug(PublicTransitComponent.LogTag, $"Asked for {count} matches for `{input}`: No results (how?)");
			} else {
				Logger.Debug(PublicTransitComponent.LogTag, $"Asked for {count} matches for `{input}`: best result is {matches.First!.Value.Station.DisplayName} with {matches.First.Value.Score}, worst is {matches.Last!.Value.Station.DisplayName} with {matches.Last.Value.Score}");
			}

			return matches.ToArray();
		}

		public StationMatchInfo Lookup(string input) {
			return Lookup(input, 1)[0];
		}
	}
}
