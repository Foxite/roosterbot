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
			return Name.ToLower() == input;
		}
	}
}