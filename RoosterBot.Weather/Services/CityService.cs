using CsvHelper;
using System;
using System.Collections.Concurrent;
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
					// The only reason this is a ConcurrentDictionary is because it has a GetOrAdd function which Dictionary does not, for some reason.
					ConcurrentDictionary<int, RegionInfo> m_Regions = new ConcurrentDictionary<int, RegionInfo>();
					m_Cities = new List<CityInfo>();

					await csv.ReadAsync();
					csv.ReadHeader();

					while (await csv.ReadAsync()) {
						RegionInfo region = m_Regions.GetOrAdd(int.Parse(csv["state_code"]), id => new RegionInfo(id, csv["state_name"]));
						CityInfo city = new CityInfo(
							int.Parse(csv["city_id"]),
							csv["city_name"],
							region
						);
						m_Cities.Add(city);
					}
				}
			}
		}

		public Task<CityInfo> GetByWeatherBitIdAsync(int weatherBitId) => Task.Run(() => {
			return m_Cities.SingleOrDefault(city => city.CityId == weatherBitId);
		});

		public Task<CityInfo> Lookup(string input) => Task.Run(() => {
			// TODO implement better matching logic
			// TODO find a way to differentiate between identical city names in different states (Hengelo being a good example)
			input = Util.RemoveDiacritics(input).ToLower();
			foreach (CityInfo city in m_Cities) {
				if (city.Match(input)) {
					return city;
				}
			}
			return null;
		});
	}
}