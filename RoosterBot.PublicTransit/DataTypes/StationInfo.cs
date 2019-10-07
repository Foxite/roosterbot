using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfo {
		public string Code { get; }
		public string ShortName { get; }
		public string MidName { get; }
		public string LongName { get; }
		public float Latitude { get; }
		public float Longitude { get; }
		public IReadOnlyList<string> Synonyms { get; }

		public string DisplayName => LongName ?? MidName ?? ShortName ?? Code ?? "If you see this, please contact the bot owner for a free mars bar.";

		private IEnumerable<string> Names => Synonyms.Concat(new[] { ShortName, MidName, LongName }.Where(item => item != null));

		internal StationInfo(XElement xmlStation) {
			Code = xmlStation.Element(XName.Get("Code")).Value;
			IEnumerable<XElement> names =
				from name in xmlStation.Element(XName.Get("Namen")).Elements()
				select name;

			string getXmlName(string identifier) {
				return names.SingleOrDefault(element => element.Name == identifier)?.Value;
			}
			
			ShortName = getXmlName("Kort");
			MidName = getXmlName("Middel");
			LongName = getXmlName("Lang");

			XElement synos = xmlStation.Element(XName.Get("Synoniemen"));
			if (synos == null) {
				Synonyms = new List<string>().AsReadOnly();
			} else {
				Synonyms = (from syno in synos.Elements()
							select syno.Value).ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Lower return value means better score.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Match(string input) {
			int score = PublicTransitUtil.Levenshtein(input, Names.First().ToLower());
			foreach (string name in Names.Skip(1).Select(name => name.ToLower())) {
				score = Math.Min(score, PublicTransitUtil.Levenshtein(input, name));
			}
			return score;
		}
	}

	public class StationMatchInfo : IComparable<StationMatchInfo> {
		public StationInfo Station { get; set; }
		public float Score { get; set; }

		public int CompareTo(StationMatchInfo other) {
			return Math.Sign(Score - other.Score);
		}
	}
}
