using System;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class WeatherService {
		private readonly string m_WeatherBitKey;

		public WeatherService(string weatherBitKey) {
			m_WeatherBitKey = weatherBitKey;
		}

		public Task<WeatherInfo> GetWeatherForecastAsync(CityInfo city, TimeSpan timeFromNow) => GetWeatherForecastAsync(city, DateTime.Now + timeFromNow);
		public Task<WeatherInfo> GetWeatherForecastAsync(CityInfo city, DateTime dateTime) => throw new NotImplementedException();
	}

	public class WeatherInfo {
		public CityInfo City { get; }

		/// <summary>
		/// Celcius
		/// </summary>
		public float Temperature { get; }

		// TODO properties with wind, pressure, humidity, precipation and whatnot
		// TODO constructor, ideally it will take the API response and fill itself in, so we won't have to assign 20 parameters

		public string Present() {
			return $"{City.Name}, {City.Region}: {Math.Round(Temperature, 1)} °C";
		}
	}
}