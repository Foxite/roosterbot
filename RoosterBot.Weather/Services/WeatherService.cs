﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Weather {
	// TODO (hold) (refactor) this should be split into a full WeatherBit C# api, which could actually become an entirely new library that this component uses
	public class WeatherService : IDisposable {
		private const string BaseUrl = "https://api.weatherbit.io/v2.0/";

		private readonly string m_WeatherBitKey;
		private readonly WebClient m_Web;
		private readonly ResourceService m_Resources;

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
			}))!["data"]![0]!.ToObject<JObject>()!);
		}

		public async Task<WeatherInfo> GetWeatherForecastAsync(CityInfo city, int hoursFromNow) {
			return new WeatherInfo(m_Resources, this, city, (await GetResponseAsync("forecast/hourly", new Dictionary<string, string>() {
				{ "city_id", city.CityId.ToString() },
				{ "hours", hoursFromNow.ToString() }
			}))!["data"]!.Last!.ToObject<JObject>()!);
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
