using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Weather {
	public class WeatherInfo {
		// Weather code from WeatherBit
		private short m_WeatherCode;
		private WeatherService m_WeatherService;
		private ResourceService m_Resources;

		public CityInfo City { get; }

		/// <summary>
		/// Celcius
		/// </summary>
		public float Temperature { get; }
		public float ApparentTemperature { get; }

		/// <summary>
		/// km/h
		/// </summary>
		public float WindSpeed { get; }

		public string WindDirection { get; }

		internal WeatherInfo(ResourceService resources, WeatherService service, CityInfo city, JObject jsonInfo) {
			m_Resources = resources;
			m_WeatherService = service;
			City = city;

			Temperature = jsonInfo["temp"].ToObject<float>();
			ApparentTemperature = jsonInfo["app_temp"].ToObject<float>();

			WindSpeed = jsonInfo["wind_spd"].ToObject<float>() * 3.6f; // m/s -> km/h
			WindDirection = jsonInfo["wind_cdir_full"].ToObject<string>();

			m_WeatherCode = jsonInfo["weather"]["code"].ToObject<short>();
		}

		/// <summary>
		/// Format the WeatherInfo to be sent to Discord, including a pretext with city and time information.
		/// </summary>
		public string Present(DateTime datetime, CultureInfo culture, bool useMetric) {
			string ret;

			if (City.Name == City.Region.Name) {
				ret = $"{City.Name}: Weer ";
			} else {
				ret = $"{City.Name}, {City.Region}: Weer ";
			}

			if ((datetime - DateTime.Now).TotalMinutes < 1) {
				ret += "nu";
			} else {
				ret += DateTimeUtil.GetRelativeDateReference(datetime.Date, culture) + " " + datetime.ToShortTimeString(culture);
			}
			ret += "\n";
			ret += Present(culture, useMetric);
			return ret;
		}

		/// <summary>
		/// Format the WeatherInfo to be sent to Discord.
		/// </summary>
		public string Present(CultureInfo culture, bool useMetric) {
			string ret = ":thermometer: ";
			if (useMetric) {
				ret += Math.Round(Temperature, 1).ToString() + " °C";
			} else {
				ret += Math.Round(Temperature * 9 / 5 + 32, 1).ToString() + " °F";
			}

			if (ApparentTemperature != Temperature) {
				string appTempString;
				if (useMetric) {
					appTempString = Math.Round(Temperature, 1).ToString() + " °C";
				} else {
					appTempString = Math.Round(Temperature * 9 / 5 + 32, 1).ToString() + " °F";
				}

				ret += string.Format(m_Resources.GetString(culture, "WeatherInfo_Present_ApparentTemperature"), appTempString);
			}
			ret += "\n" + m_WeatherService.GetDescription(culture, m_WeatherCode) + "\n";

			if (WindSpeed == 0) {
				ret += m_Resources.GetString(culture, "WeatherInfo_Present_NoWind");
			} else {
				string windSpeedString;
				if (useMetric) {
					windSpeedString = Math.Round(WindSpeed, 1).ToString() + " km/h";
				} else {
					windSpeedString = Math.Round(WindSpeed * 1.609, 1).ToString() + " mph";
				}

				ret += string.Format(m_Resources.GetString(culture, "WeatherInfo_Present_Wind"), windSpeedString, WindDirection);
			}
			return ret;
		}
	}
}
