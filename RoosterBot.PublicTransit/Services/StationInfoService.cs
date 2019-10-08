using System;
using System.Collections.Generic;
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

		public IReadOnlyList<StationMatchInfo> Lookup(string input, int count) {
			string inputLower = input.ToLower();
			List<StationMatchInfo> matches = new List<StationMatchInfo>(count);
			
			void insertMatch(StationMatchInfo match) {
				int insertPosition = 0;

				for (int i = 1; i < matches.Count; i++) {
					if (matches[insertPosition].Score < matches[i].Score) {
						insertPosition = i;
					} else {
						break;
					}
				}

				if (insertPosition < count) {
					matches.Insert(0, match);
				}
			}

			foreach (StationInfo v in m_Stations) {
				insertMatch(new StationMatchInfo() { Station = v, Score = v.Match(inputLower) });
			}

			if (count == 1) {
				Logger.Debug("SCS", $"Asked for 1 for `{input}`: result is {matches[0].Station.DisplayName} with {matches[0].Score}");
			} else {
				Logger.Debug("SCS", $"Asked for {count} matches for `{input}`: best result is {matches[0].Station.DisplayName} with {matches[0].Score}, worst is {matches[count - 1].Station.DisplayName} with {matches[count - 1].Score}");
			}

			return matches;
		}

		public StationMatchInfo Lookup(string input) {
			return Lookup(input, 1)[0];
		}
	}
}
