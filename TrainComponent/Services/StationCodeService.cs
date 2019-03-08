using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RoosterBot;

namespace PublicTransitComponent.Services {
	public class StationCodeService {
		private List<StationInfo> m_Stations;

		public StationInfo DefaultDeparture { get; private set; }

		public StationCodeService(string stationXmlPath, string defaultDepartureCode) {
			m_Stations = new List<StationInfo>();

			XElement xml = XElement.Load(stationXmlPath);
			
			foreach (XElement xStation in xml.Elements()) {
				string code = xStation.Element(XName.Get("Code")).Value;
				IEnumerable<string> names =
					from name in xStation.Element(XName.Get("Namen")).Elements()
					select name.Value;

				string displayName = names.Last();

				XElement synos = xStation.Element(XName.Get("Synoniemen"));
				if (synos != null) {
					names = names.Concat(from syno in synos.Elements()
								 select syno.Value);
				}

				StationInfo station = new StationInfo(code, displayName, names.Select(n => n.ToLower()).ToArray());
				m_Stations.Add(station);

				if (code == defaultDepartureCode) {
					DefaultDeparture = station;
				}
			}
		}

		public StationInfo Lookup(string input) {
			string inputLower = input.ToLower();
			int bestMatchScore = m_Stations[0].Match(inputLower);
			StationInfo bestMatch = m_Stations[0];

			for (int i = 1; i < m_Stations.Count; i++) {
				int score = m_Stations[i].Match(inputLower);
				if (score < bestMatchScore) {
					bestMatchScore = score;
					bestMatch = m_Stations[i];
				}

				if (bestMatchScore == 0) {
					break;
				}
			}

			Logger.Debug("SCS", $"`{input}` matched to `{bestMatch.DisplayName}` ({bestMatch.Code}) with a score of {bestMatchScore}");

			return bestMatch;
		}
	}

	public class StationInfo {
		public readonly string Code;
		public readonly string DisplayName;
		public readonly string[] Names;

		public StationInfo(string code, string displayName, params string[] names) {
			Code = code;
			DisplayName = displayName;
			Names = names;
		}

		public int Match(string input) {
			int score = Util.Levenshtein(input, Names[0]);
			for (int i = 1; i < Names.Length; i++) {
				score = Math.Min(score, Util.Levenshtein(input, Names[i]));
			}
			return score;
		}
	}
}
