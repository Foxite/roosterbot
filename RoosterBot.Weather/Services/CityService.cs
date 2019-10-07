using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class CityService {
		private readonly string m_ConfigPath;
		private List<CityInfo> m_Cities;

		public CityService(string configPath) {
			m_ConfigPath = configPath;
		}

		public async Task ReadCityCSVAsync() {
			string csvPath = Path.Combine(m_ConfigPath, "cities.csv");
			using (StreamReader reader = File.OpenText(csvPath)) {
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					m_Cities = new List<CityInfo>();
					await csv.ReadAsync();
					csv.ReadHeader();
					while (await csv.ReadAsync()) {
						CityInfo city = new CityInfo(
							int.Parse(csv["city_id"]),
							int.Parse(csv["state_code"]),
							csv["city_name"],
							csv["state_name"]
						);
						m_Cities.Add(city);
					}
				}
			}
		}

		public Task<CityInfo> GetByWeatherBitIdAsync(int weatherBitId) => Task.Run(() => {
			return m_Cities.SingleOrDefault(city => city.CityId == weatherBitId);
		});

		public Task<CityInfo[]> Lookup(string input) => Task.Run(() => {
			// TODO implement better matching logic
			// TODO find a way to differentiate between identical city names in different states (Hengelo being a good example)
			foreach (CityInfo city in m_Cities) {
				if (city.Match(input)) {
					return new[] { city };
				}
			}
			return Array.Empty<CityInfo>();
		});
	}
}