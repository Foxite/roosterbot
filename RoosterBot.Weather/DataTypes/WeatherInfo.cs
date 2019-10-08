using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Weather {
	public class WeatherInfo {
		// Weather code from WeatherBit
		private short m_WeatherCode;

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

		internal WeatherInfo(CityInfo city, JObject jsonInfo) {
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
		public string Present(DateTime time, CultureInfo culture) {
			return $"{City.Name}, {City.Region}: Weer {DateTimeUtil.GetRelativeDateReference(time, culture)} {time.ToShortTimeString(culture)}\n" + Present();
		}

		/// <summary>
		/// Format the WeatherInfo to be sent to Discord.
		/// </summary>
		public string Present() {
			string ret = $":thermometer: {Math.Round(Temperature, 1)} °C";

			if (ApparentTemperature != Temperature) {
				ret += $" (voelt als {Math.Round(ApparentTemperature, 1)})";
			}
			ret += "\n";
			ret += GetDescription();

			double roundWindSpeed = Math.Round(WindSpeed, 1);
			if (roundWindSpeed < 0.05) { // For floating point errors
				ret += "\n:wind_blowing_face: Geen wind";
			} else {
				ret += $"\n:wind_blowing_face: {roundWindSpeed} km/h vanuit {WindDirection}";
			}
			return ret;
		}

		private string GetDescription() {
			switch (m_WeatherCode) {
				case 200: return ":cloud_lightning: Onweer met lichte regen";
				case 201: return ":thunder_cloud_rain: Onweer met regen";
				case 202: return ":thunder_cloud_rain: Onweer met zware regen";
				case 230: return ":cloud_lightning: Onweer met lichte motregen";
				case 231: return ":thunder_cloud_rain: Onweer met motregen";
				case 232: return ":thunder_cloud_rain: Onweer met hevige motregen";
				case 233: return ":thunder_cloud_rain: Onweer met hagel";
				case 300: return ":white_sun_rain_cloud: Lichte motregen";
				case 301: return ":cloud_rain: Motregen";
				case 302: return ":cloud_rain: Hevige motregen";
				case 500: return ":white_sun_rain_cloud: Lichte regen";
				case 501: return ":cloud_rain: Regen";
				case 502: return ":cloud_rain: Zware regen";
				case 511: return ":cloud_rain: Vriezende regen";
				case 520: return ":white_sun_rain_cloud: Lichte buien";
				case 521: return ":cloud_rain: Buien";
				case 522: return ":cloud_rain: Hevige buien";
				case 600: return ":cloud_snow: Lichte sneeuw";
				case 601: return ":cloud_snow: Sneeuw";
				case 602: return ":cloud_snow: Hevige sneeuw";
				case 610: return ":cloud_snow: Sneeuw en regen";
				case 611: return ":snowflake: Ijzel";
				case 612: return ":snowflake: Hevige ijzel";
				case 621: return ":cloud_snow: Sneeuwbuien";
				case 622: return ":cloud_snow: Zware sneeuwbuien";
				case 623: return ":cloud_snow: Sneeuwvlagen";
				case 700: return ":foggy: Mist";
				case 711: return ":warning: Rook";
				case 721: return ":foggy: Nevel";
				case 731: return "warning: Zand/stof";
				case 741: return ":foggy: Mist";
				case 751: return "foggy: :snowflake: Vriezende mist";
				case 800: return ":sunny: Klare lucht";
				case 801: return ":white_sun_small_cloud: Enkele wolken";
				case 802: return ":white_sun_small_cloud: Licht bewolkt";
				case 803: return ":white_sun_cloud: Overwegend bewolkt";
				case 804: return ":cloud: Bewolkt";
				case 900: return ":question: Onbekend"; // TODO roosterbot unknown emote
				default:
					Logger.Error("WeatherInfo", "Unknown code " + m_WeatherCode);
					return "Gefeliciteerd, je krijg een gratis Mars-reep van de bot eigenaar. Foutcode D" + m_WeatherCode;
			}
		}
	}
}
