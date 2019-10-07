namespace RoosterBot.Weather {
	public class CityInfo {
		public int WeatherBitId { get; }
		public string Name { get; }
		public string Region { get; }

		public CityInfo(int weatherBitId, string name, string region) {
			WeatherBitId = weatherBitId;
			Name = name;
			Region = region;
		}
	}
}