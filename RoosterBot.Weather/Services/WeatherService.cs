using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class WeatherService : IDisposable {
		private readonly string m_WeatherBitKey;
		private const string BaseUrl = "https://api.weatherbit.io/v2.0/";
		private WebClient m_Web;

		public WeatherService(string weatherBitKey) {
			m_WeatherBitKey = weatherBitKey;
			m_Web = new WebClient();
		}

		public async Task<WeatherInfo> GetCurrentWeatherAsync(CityInfo city) {
			return new WeatherInfo(city, await GetResponseAsync("current", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() }
			}));
		}

		public async Task<WeatherInfo> GetWeatherForecastAsync(CityInfo city, int hoursFromNow) {
			return new WeatherInfo(city, await GetResponseAsync("forecast/hourly", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() },
				{ "hours", hoursFromNow.ToString() + "-" + (hoursFromNow + 1).ToString() }
			}));
		}

		private async Task<JObject> GetResponseAsync(string function, IDictionary<string, string> parameters) {
			string url = BaseUrl + function;
			url += "?key=" + m_WeatherBitKey;
			foreach (KeyValuePair<string, string> parameter in parameters) {
				url += $"&{parameter.Key}={parameter.Value}";
			}

			string result = await m_Web.DownloadStringTaskAsync(url);

			return JObject.Parse(result);
		}

		#region IDisposable Support
		private bool m_DisposedValue = false;

		protected virtual void Dispose(bool disposing) {
			if (!m_DisposedValue) {
				if (disposing) {
					m_Web.Dispose();
				}

				m_Web = null;

				m_DisposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
