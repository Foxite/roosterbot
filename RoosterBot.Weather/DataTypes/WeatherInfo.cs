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
				case 200:
					return "Onweer met lichte regen";
				case 201:
					return "Onweer met regen";
				case 202:
					return "Onweer met zware regen";
				case 230:
					return "Onweer met lichte motregen";
				case 231:
					return "Onweer met motregen";
				case 232:
					return "Onweer met hevige motregen";
				case 233:
					return "Onweer met hagel";
				case 300:
				case 301:
				case 302:
				case 500:
				case 501:
				case 502:
				case 511:
				case 520:
				case 521:
				case 522:
				case 600:
				case 601:
				case 602:
				case 611:
				case 612:
				case 621:
				case 622:
				case 623:
				case 700:
				case 711:
				case 721:
				case 731:
				case 741:
				case 751:
				case 800:
				case 801:
				case 802:
				case 803:
				case 804:
					return "TODO";
				case 900:
					return "Onbekend";
				default:
					return "Gefeliciteerd, je krijg een gratis Mars-reep van de bot eigenaar. Foutcode D" + m_WeatherCode;
			}
		}
	}
}
