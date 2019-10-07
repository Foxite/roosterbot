using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class CityService {
		private readonly string m_ConfigPath;
		private List<CityInfo> m_Cities;

		public CityService(string configPath) {
			m_ConfigPath = configPath;
		}

		public Task ReadCityCSVAsync() {
			// TODO read CSV data and parse into list
			return Task.CompletedTask;
		}

		public Task<CityInfo> GetByWeatherBitId(int weatherBitId) => Task.Run(() => {
			return m_Cities.SingleOrDefault(city => city.WeatherBitId == weatherBitId);
		});

		public Task<CityInfo[]> Lookup(string input) => Task.Run(() => {
			// TODO implement matching logic and search the list
			return Array.Empty<CityInfo>();
		});
	}
}