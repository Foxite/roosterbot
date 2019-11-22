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
			m_Cities = new List<CityInfo>();
		}

		public async Task ReadCityCSVAsync() {
			string csvPath = Path.Combine(m_ConfigPath, "cities.csv");
			using StreamReader reader = File.OpenText(csvPath);
			using CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," });

			Dictionary<int, RegionInfo> regions = new Dictionary<int, RegionInfo>();

			await csv.ReadAsync();
			csv.ReadHeader();

			while (await csv.ReadAsync()) {
				int stateCode = int.Parse(csv["state_code"]);
				if (!regions.TryGetValue(stateCode, out RegionInfo? region)) {
					region = new RegionInfo(stateCode, csv["state_name"]);
					regions[stateCode] = region;
				}
				CityInfo city = new CityInfo(
					int.Parse(csv["city_id"]),
					csv["city_name"],
					region,
					csv["alt_city_names"].Split("|")
				);
				m_Cities.Add(city);
			}
		}

		public Task<CityInfo?> GetByWeatherBitIdAsync(int weatherBitId) => Task.Run(() => {
			// Apparently, SingleOrDefault returns `T`, not `T?`, allowing it to pass null into non-nullable targets.
			// I'm pretty sure this is to do with the fact that LINQ functions have to work with all objects, including structs, which cannot be made nullable in the same way as classes.
			// Either way the warning can be fixed by casting the return value to a nullable type.
			return (CityInfo?) m_Cities.SingleOrDefault(city => city.CityId == weatherBitId);
		});

		public Task<CityInfo?> Lookup(string input) => Task.Run(() => {
			input = StringUtil.RemoveDiacritics(input).ToLower();
			foreach (CityInfo city in m_Cities) {
				if (city.Match(input)) {
					return city;
				}
			}
			return null;
		});
	}
}