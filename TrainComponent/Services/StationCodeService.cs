using System;
using System.Collections.Generic;
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

		public StationInfo GetByCode(string code) {
			return m_Stations.Find(info =>  info.Code == code);
		}

		public StationMatchInfo[] Lookup(string input, int count) {
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

	public class StationMatchInfo : IComparable<StationMatchInfo> {
		public StationInfo Station { get; set; }
		public int Score { get; set; }

		public int CompareTo(StationMatchInfo other) {
			return Score - other.Score;
		}
	}
}
