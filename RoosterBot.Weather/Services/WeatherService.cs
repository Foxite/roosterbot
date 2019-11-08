using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class WeatherService : IDisposable {
		private readonly string m_WeatherBitKey;
		private const string BaseUrl = "https://api.weatherbit.io/v2.0/";
		private WebClient m_Web;
		private ResourceService m_Resources;

		/// <summary>
		/// Indicates if the WeatherBit API is being used under a Free license, which means attribution must be provided.
		/// </summary>
		public bool Attribution { get; }

		public WeatherService(ResourceService resources, string weatherBitKey, bool attribution) {
			m_WeatherBitKey = weatherBitKey;
			m_Resources = resources;
			m_Web = new WebClient();
			Attribution = attribution;
		}

		public async Task<WeatherInfo> GetCurrentWeatherAsync(CityInfo city) {
			return new WeatherInfo(m_Resources, this, city, (await GetResponseAsync("current", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() }
			}))["data"][0].ToObject<JObject>());
		}

		public async Task<WeatherInfo> GetWeatherForecastAsync(CityInfo city, int hoursFromNow) {
			return new WeatherInfo(m_Resources, this, city, (await GetResponseAsync("forecast/hourly", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() },
				{ "hours", hoursFromNow.ToString() }
			}))["data"].Last.ToObject<JObject>());
		}

		public async Task<WeatherInfo[]> GetDayForecastAsync(CityInfo city, DateTime date) {
			// TODO (Feature) Change DayForecast to return a single info item for that whole day
			// The API does not allow us to see per-hour beyond 48 hours (unless you pay), but it does let us see per-day up to 5 days in the future.
			// If you try to view further than 48 hours it will simply not return any more data, causing an exception when we try to get the last few items.
			int hoursForecast = (int) (date - DateTime.Today).TotalHours + 18;

			JToken result = (await GetResponseAsync("forecast/hourly", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() },
				{ "hours", hoursForecast.ToString() }
			}))["data"];

			JToken[] resultIntervals = new[] {
				result[hoursForecast - 11], // 8 am
				result[hoursForecast - 7], // 12 pm
				result[hoursForecast - 1] // 6 pm
			};

			return resultIntervals.Select(jt => new WeatherInfo(m_Resources, this, city, jt.ToObject<JObject>())).ToArray();
		}

		public string GetDescription(CultureInfo culture, short weatherCode) {
			string ret = m_Resources.GetString(culture, "WeatherBit_Code_" + weatherCode);

			if (ret == null) {
				Logger.Error("WeatherInfo", "Unknown code " + weatherCode);
				return m_Resources.GetString(culture, "WeatherBit_Code_Unknown");
			} else {
				return ret;
			}
		}

		private async Task<JObject> GetResponseAsync(string function, IDictionary<string, string> parameters) {
			string url = BaseUrl + function;
			url += $"?key={m_WeatherBitKey}&lang=nl";
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

				m_DisposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
