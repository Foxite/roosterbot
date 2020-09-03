using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Foxite.Common;

namespace RoosterBot.Weather {
	public class CityService {
		private readonly string m_ConfigPath;
		private readonly List<CityInfo> m_Cities;

		public CityService(string configPath) {
			m_ConfigPath = configPath;
			m_Cities = new List<CityInfo>();
		}

		public void ReadCityCSV() {
			Logger.Info("CityService", "Loading cities.csv");

			string csvPath = Path.Combine(m_ConfigPath, "cities.csv");
			using StreamReader reader = File.OpenText(csvPath);
			using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," });

			var regions = new Dictionary<int, RegionInfo>();

			csv.Read();
			csv.ReadHeader();

			while (csv.Read()) {
				int stateCode = int.Parse(csv["state_code"]);
				if (!regions.TryGetValue(stateCode, out RegionInfo? region)) {
					region = new RegionInfo(stateCode, csv["state_name"]);
					regions[stateCode] = region;
				}
				var city = new CityInfo(
					int.Parse(csv["city_id"]),
					csv["city_name"],
					region,
					csv["alt_city_names"].Split("|")
				);
				m_Cities.Add(city);
			}
			Logger.Info("CityService", "Finished loading cities.csv");
		}

		public Task<CityInfo?> GetByWeatherBitIdAsync(int weatherBitId) => Task.Run(() => {
			// Apparently, SingleOrDefault returns `T`, not `T?`, allowing it to pass null into non-nullable targets.
			// I'm pretty sure this is to do with the fact that LINQ functions have to work with all objects, including structs, which cannot be made nullable in the same way as classes.
			// Either way the warning can be fixed by casting the return value to a nullable type.
			return (CityInfo?) m_Cities.SingleOrDefault(city => city.CityId == weatherBitId);
		});

		public Task<CityInfo?> Lookup(string input) => Task.Run(() => {
			input = input.RemoveDiacritics().ToLower();
			return (CityInfo?) m_Cities.FirstOrDefault(city => city.Match(input));
		});
	}
}