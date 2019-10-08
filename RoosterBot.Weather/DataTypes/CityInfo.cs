using System.Linq;

namespace RoosterBot.Weather {
	public class CityInfo {
		public int CityId { get; }
		public int RegionId { get; }

		public string Name { get; }
		public string Region { get; }

		public CityInfo(int cityId, int regionId, string name, string region) {
			CityId = cityId;
			RegionId = regionId;
			Name = name;
			Region = region;
		}

		public bool Match(string input) {
			// TODO ignore diacritics when matching (Frisian towns contain a lot of carets)
			string[] split = input.Split(',');
			if (split.Length == 1) {
				return MatchName(input);
			} else {
				string inputRegion = split[split.Length - 1].Trim();
				string inputName = string.Join(" ", split.Take(split.Length - 1).Select(str => str.Trim()));
				return Region.ToLower().StartsWith(inputRegion) && MatchName(inputName);
			}
		}

		private bool MatchName(string input) {
			return Name.ToLower() == input;
		}
	}
}