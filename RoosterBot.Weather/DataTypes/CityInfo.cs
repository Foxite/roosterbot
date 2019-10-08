using System.Linq;

namespace RoosterBot.Weather {
	public class CityInfo {
		public int CityId { get; }

		public string Name { get; }
		public RegionInfo Region { get; }

		private string m_NormalName;

		public CityInfo(int cityId, string name, RegionInfo region) {
			CityId = cityId;
			Name = name;
			Region = region;

			m_NormalName = Util.RemoveDiacritics(name).ToLower();
		}

		public bool Match(string input) {
			string[] split = input.Split(',');
			if (split.Length == 1) {
				return MatchName(input);
			} else {
				return
					Region.Match(split[split.Length - 1].Trim()) &&
					MatchName(string.Join(" ", split.Take(split.Length - 1).Select(str => str.Trim())));
			}
		}

		private bool MatchName(string input) {
			// We could change this to a fuzzy matching algorithm, but is it necessary?
			return m_NormalName == input;
		}

		public override string ToString() => Name;
	}
}