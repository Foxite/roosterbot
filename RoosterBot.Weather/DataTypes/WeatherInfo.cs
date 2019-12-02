using System;
using System.Collections.Generic;
using System.Globalization;
using Discord;
using Newtonsoft.Json.Linq;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Weather {
	public class WeatherInfo {
		private readonly WeatherService m_WeatherService;
		private readonly ResourceService m_Resources;

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

		/// <summary>
		/// The code used by WeatherBit to indicate the type of weather.
		/// </summary>
		public short WeatherCode { get; }

		internal WeatherInfo(ResourceService resources, WeatherService service, CityInfo city, JObject jsonInfo) {
			m_Resources = resources;
			m_WeatherService = service;
			City = city;

			Temperature = jsonInfo["temp"].ToObject<float>();
			ApparentTemperature = jsonInfo["app_temp"].ToObject<float>();

			WindSpeed = jsonInfo["wind_spd"].ToObject<float>() * 3.6f; // m/s -> km/h
			WindDirection = jsonInfo["wind_cdir_full"].ToObject<string>();

			WeatherCode = jsonInfo["weather"]["code"].ToObject<short>();
		}

		/// <summary>
		/// Creates an AspectListResult that contains all information from <see cref="Present(CultureInfo, bool)"/>, as well as a pretext with the City and Region name and the DateTime.
		/// </summary>
		public AspectListResult Present(DateTime datetime, CultureInfo culture, bool useMetric) {
			string pretext;

			if (City.Name == City.Region.Name) {
				pretext = $"{City.Name}: Weer ";
			} else {
				pretext = $"{City.Name}, {City.Region}: Weer ";
			}

			if ((datetime - DateTime.Now).TotalMinutes < 1) {
				pretext += "nu";
			} else {
				pretext += DateTimeUtil.GetRelativeDateReference(datetime.Date, culture) + " " + datetime.ToString("s", culture);
			}
			return Present(pretext, culture, useMetric);
		}

		public AspectListResult Present(string caption, CultureInfo culture, bool useMetric) {
			IEnumerable<AspectListItem> getAspects() {
				string temperature;
				if (useMetric) {
					temperature = Math.Round(Temperature, 1).ToString() + " °C";
				} else {
					temperature = Math.Round(Temperature * 9 / 5 + 32, 1).ToString() + " °F";
				}

				if (ApparentTemperature != Temperature) {
					string appTempString;
					if (useMetric) {
						appTempString = Math.Round(Temperature, 1).ToString() + " °C";
					} else {
						appTempString = Math.Round(Temperature * 9 / 5 + 32, 1).ToString() + " °F";
					}

					temperature += string.Format(m_Resources.GetString(culture, "WeatherInfo_Present_ApparentTemperature"), appTempString);
				}
				yield return new AspectListItem(new Emoji("🌡️"), m_Resources.GetString(culture, "WeatherInfo_Present_TemperatureAspect"), temperature);
				yield return new AspectListItem(WeatherCode switch
				{
					200 => new Emoji("🌩️"),
					230 => new Emoji("🌩️"),
					201 => new Emoji("⛈️"),
					202 => new Emoji("⛈️"),
					231 => new Emoji("⛈️"),
					232 => new Emoji("⛈️"),
					233 => new Emoji("⛈️"),
					300 => new Emoji("🌦️"),
					500 => new Emoji("🌦️"),
					520 => new Emoji("🌦️"),
					301 => new Emoji("🌧️"),
					302 => new Emoji("🌧️"),
					501 => new Emoji("🌧️"),
					502 => new Emoji("🌧️"),
					511 => new Emoji("🌧️"),
					521 => new Emoji("🌧️"),
					522 => new Emoji("🌧️"),
					600 => new Emoji("🌨️"),
					601 => new Emoji("🌨️"),
					602 => new Emoji("🌨️"),
					610 => new Emoji("🌨️"),
					621 => new Emoji("🌨️"),
					622 => new Emoji("🌨️"),
					623 => new Emoji("🌨️"),
					611 => new Emoji("❄️"),
					612 => new Emoji("❄️"),
					711 => new Emoji("⚠️"),
					731 => new Emoji("⚠️"),
					700 => new Emoji("🌫️"),
					721 => new Emoji("🌫️"),
					741 => new Emoji("🌫️"),
					751 => new Emoji("🌫️"),
					800 => new Emoji("☀️"),
					801 => new Emoji("🌤️"),
					802 => new Emoji("🌤️"),
					803 => new Emoji("⛅"),
					804 => new Emoji("☁️"),
					900 => Emote.Parse("<:unknown:636213624460935188>"),
					_   => Emote.Parse("<:error:636213609919283238>")
				}, m_Resources.GetString(culture, "WeatherInfo_Present_WeatherAspect"), m_WeatherService.GetDescription(culture, WeatherCode));

				if (WindSpeed == 0) {
					yield return new AspectListItem(new Emoji("🌬️"), m_Resources.GetString(culture, "WeatherInfo_Present_WindAspect"), m_Resources.GetString(culture, "WeatherInfo_Present_NoWind"));
				} else {
					string windSpeedString;
					if (useMetric) {
						windSpeedString = Math.Round(WindSpeed, 1).ToString() + " km/h";
					} else {
						windSpeedString = Math.Round(WindSpeed * 1.609, 1).ToString() + " mph";
					}

					yield return new AspectListItem(new Emoji("🌬️"), m_Resources.GetString(culture, "WeatherInfo_Present_WindAspect"), windSpeedString);
				}
			}

			return new AspectListResult(caption, getAspects());
		}
	}
}
