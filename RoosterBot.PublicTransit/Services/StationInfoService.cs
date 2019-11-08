using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfoService {
		private List<StationInfo> m_Stations;

		public StationInfo DefaultDeparture { get; private set; }

#pragma warning disable CS8618
		// Non-nullable field is uninitialized. Consider declaring as nullable.
		// This warning is incorrect, see bug report: https://github.com/dotnet/roslyn/issues/39740
		// The constructor will throw if DefaultDeparture ends up uninitialized, but the compiler doesn't know that.
		public StationInfoService(string stationXmlPath, string defaultDepartureCode) {
#pragma warning restore CS8618
			m_Stations = new List<StationInfo>();

			XElement xml = XElement.Load(stationXmlPath);
			
			foreach (XElement xStation in xml.Elements()) {
				StationInfo station = new StationInfo(xStation);
				m_Stations.Add(station);

				if (station.Code == defaultDepartureCode) {
					DefaultDeparture = station;
				}
			}

			if (DefaultDeparture == null) {
				throw new InvalidOperationException("No station matching the default departure code was found in the xml file.");
			}
		}

		public StationInfo? GetByCode(string code) {
			return m_Stations.Find(info =>  info.Code == code);
		}

		public StationMatchInfo[] Lookup(string input, int count) {
			string inputLower = input.ToLower();
			LinkedList<StationMatchInfo> matches = new LinkedList<StationMatchInfo>();
			
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
				Logger.Debug("SCS", $"Asked for 1 for `{input}`: result is {matches.First!.Value.Station.DisplayName} with {matches.First.Value.Score}");
			} else if (count == 0) {
				Logger.Debug("SCS", $"Asked for {count} matches for `{input}`: No results (how?)");
			} else {
				Logger.Debug("SCS", $"Asked for {count} matches for `{input}`: best result is {matches.First!.Value.Station.DisplayName} with {matches.First.Value.Score}, worst is {matches.Last!.Value.Station.DisplayName} with {matches.Last.Value.Score}");
			}

			return matches.ToArray();
		}

		public StationMatchInfo Lookup(string input) {
			return Lookup(input, 1)[0];
		}
	}
}
