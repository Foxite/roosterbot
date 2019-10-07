using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	// TODO localize? The API is not restricted to the Netherlands but we would have to provide the entire cities file, which takes considerable time to read
	// Alternatively we could provide a cities file with only the cities in countries we are localized to

	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("weer")]
	public class WeatherModule : RoosterModuleBase {
		public WeatherService Weather { get; set; }

		[Command(RunMode = RunMode.Async), Alias("nu")]
		public async Task GetCurrentWeatherCommand(CityInfo city) {
			WeatherInfo weather = await Weather.GetWeatherForecastAsync(city, DateTime.Now);
			ReplyDeferred(weather.Present());
		}

		[Command(RunMode = RunMode.Async), Alias("dag")]
		public async Task GetDayForecastCommand(DayOfWeek day, CityInfo city) {
			// Get the forecast for the day at {hours} o clock
			DateTime date = DateTime.Today.AddDays((int) (day - DateTime.Today.DayOfWeek));
			Task<WeatherInfo> getHourForecast(int hours) {
				return Weather.GetWeatherForecastAsync(city, date.AddHours(hours));
			}
			
			WeatherInfo morning = await getHourForecast(8);
			WeatherInfo noon    = await getHourForecast(12);
			WeatherInfo evening = await getHourForecast(18);

			string response = $"{city.Name}: Weer op {day}\n";
			response +=   "08:00: " + morning.Present();
			response += "\n12:00: " + noon.Present();
			response += "\n18:00: " + evening.Present();

			ReplyDeferred(response);
		}

		// TODO Discord.NET features a builtin TimeSpan reader, however it probably won't work very well in Dutch (or any language besides English probably)
		// We should probably add a localized one to RoosterBot
		[Command("over", RunMode = RunMode.Async)]
		public async Task GetForecastCommand(TimeSpan time, CityInfo city) {
			WeatherInfo weather = await Weather.GetWeatherForecastAsync(city, time);
			ReplyDeferred(weather.Present());
		}
	}
}