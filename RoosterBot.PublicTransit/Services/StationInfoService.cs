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

		public StationMatchInfo[] Lookup(string input, int count) {
			// TODO is there a faster way to do this?
			string inputLower = input.ToLower();
			List<StationMatchInfo> matches = new List<StationMatchInfo>();

			for (int i = 0; i < m_Stations.Count; i++) {
				int score = m_Stations[i].Match(inputLower);
				matches.Add(new StationMatchInfo() { Station = m_Stations[i], Score = score });
			}

			matches.Sort();
			StationMatchInfo[] ret = matches.GetRange(0, count).ToArray();

			if (count == 1) {
				Logger.Debug("SCS", $"Asked for 1 for `{input}`: result is {ret[0].Station.DisplayName} with {ret[0].Score}");
			} else {
				Logger.Debug("SCS", $"Asked for {count} matches for `{input}`: best result is {ret[0].Station.DisplayName} with {ret[0].Score}, worst is {ret[count - 1].Station.DisplayName} with {ret[count - 1].Score}");
			}

			return ret;
		}

		public StationMatchInfo Lookup(string input) {
			return Lookup(input, 1)[0];
		}
	}
}
