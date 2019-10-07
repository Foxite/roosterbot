using System;
using Newtonsoft.Json.Linq;

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
		/// m/s
		/// </summary>
		public float WindSpeed { get; }

		public string WindDirectionAbbr { get; }

		internal WeatherInfo(CityInfo city, JObject jObject) {
			City = city;

			JObject info = jObject["data"][0].ToObject<JObject>();

			Temperature = info["temp"].ToObject<float>();
			ApparentTemperature = info["app_temp"].ToObject<float>();

			m_WeatherCode = info["weather"]["code"].ToObject<short>();
		}

		public string Present() {
			return $"{City.Name}, {City.Region}: {Math.Round(Temperature, 1)} °C\n{GetDescription()}";
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
				case 300: return "Lichte motregen";
				case 301: return "Motregen";
				case 302: return "Hevige motregen";
				case 500: return "Lichte regen";
				case 501: return "Regen";
				case 502: return "Zware regen";
				case 511: return "Vriezende regen";
				case 520: return "Lichte buien";
				case 521: return "Buien";
				case 522: return "Hevige buien";
				case 600: return "Lichte sneeuw";
				case 601: return "Sneeuw";
				case 602: return "Hevige sneeuw";
				case 610: return "Sneeuw en regen";
				case 611: return "Ijzel";
				case 612: return "Hevige ijzel";
				case 621: return "Sneeuwbuien";
				case 622: return "Zware sneeuwbuien";
				case 623: return "Sneeuwvlagen";
				case 700: return "Mist";
				case 711: return "Rook";
				case 721: return "Nevel";
				case 731: return "Zand/stof";
				case 741: return "Mist";
				case 751: return "Vriezende mist";
				case 800: return "Klare lucht";
				case 801: return "Enkele wolken";
				case 802: return "Licht bewolkt";
				case 803: return "Overwegend bewolkt";
				case 804: return "Bewolkt";
				case 900: return "Onbekend";
				default:
					Logger.Error("WeatherInfo", "Unknown code " + m_WeatherCode);
					return "Gefeliciteerd, je krijg een gratis Mars-reep van de bot eigenaar. Foutcode D" + m_WeatherCode;
			}
		}
	}
}
