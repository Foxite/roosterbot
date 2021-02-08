using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfoService {
		private readonly List<StationInfo> m_Stations;

		public StationInfo DefaultDeparture { get; private set; }

		public StationInfoService(string stationFilePath, string defaultDepartureCode) {
			Logger.Info(PublicTransitComponent.LogTag, "Loading stations file");

			m_Stations = new List<StationInfo>();

			var xml = XElement.Load(stationFilePath);

			foreach (XElement xStation in xml.Elements()) {
				var station = new StationInfo(xStation);
				m_Stations.Add(station);

				if (station.Code == defaultDepartureCode) {
					DefaultDeparture = station;
				}
			}

			if (DefaultDeparture == null) {
				throw new InvalidOperationException("No station matching the default departure code was found in the stations file.");
			}
			Logger.Info(PublicTransitComponent.LogTag, "Finished loading stations file");
		}

		public StationInfo? GetByCode(string code) {
			return m_Stations.Find(info => info.Code == code);
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

			Logger.Debug(PublicTransitComponent.LogTag, count switch
			{
				1 => $"Asked for 1 for `{input}`: result is {matches.First!.Value.Station.Name} with {matches.First!.Value.Score}",
				0 => $"Asked for {count} matches for `{input}`: No results (how?)",
				_ => $"Asked for {count} matches for `{input}`: best result is {matches.First!.Value.Station.Name} with {matches.First!.Value.Score}, worst is {matches.Last!.Value.Station.Name} with {matches.Last!.Value.Score}"
			});
			
			return matches.ToArray();
		}
	}
}
