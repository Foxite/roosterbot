using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfoService {
		private List<StationInfo> m_Stations;

		public StationInfo DefaultDeparture { get; private set; }

		public StationInfoService(string stationXmlPath, string defaultDepartureCode) {
			m_Stations = new List<StationInfo>();

			XElement xml = XElement.Load(stationXmlPath);
			
			foreach (XElement xStation in xml.Elements()) {
				StationInfo station = new StationInfo(xStation);
				m_Stations.Add(station);

				if (station.Code == defaultDepartureCode) {
					DefaultDeparture = station;
				}
			}
		}

		public StationInfo GetByCode(string code) {
			return m_Stations.Find(info =>  info.Code == code);
		}

		public StationMatchInfo[] Lookup(string input, int count) {
			string inputLower = input.ToLower();
			LinkedList<StationMatchInfo> matches = new LinkedList<StationMatchInfo>();
			
			void insertMatch(StationMatchInfo match) {
				LinkedListNode<StationMatchInfo> insertAfter = null;
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
				insertMatch(new StationMatchInfo() { Station = v, Score = v.Match(inputLower) });
			}

			if (count == 1) {
				Logger.Debug("SCS", $"Asked for 1 for `{input}`: result is {matches.First.Value.Station.DisplayName} with {matches.First.Value.Score}");
			} else {
				Logger.Debug("SCS", $"Asked for {count} matches for `{input}`: best result is {matches.First.Value.Station.DisplayName} with {matches.First.Value.Score}, worst is {matches.Last.Value.Station.DisplayName} with {matches.Last.Value.Score}");
			}

			return matches.ToArray();
		}

		public StationMatchInfo Lookup(string input) {
			return Lookup(input, 1)[0];
		}
	}
}
