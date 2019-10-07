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
}
