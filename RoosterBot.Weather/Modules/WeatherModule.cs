using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	// TODO localize? The API is not restricted to the Netherlands but we would have to provide the entire cities file, which takes considerable time to read
	// Alternatively we could provide a cities file with only the cities in countries we are localized to
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
			// Get the forecast for today at {hours} o clock
			Task<WeatherInfo> getHourForecast(int hours) {
				return Weather.GetWeatherForecastAsync(city, DateTime.Today.AddHours(hours));
			}
			
			WeatherInfo morning   = await getHourForecast(8);
			WeatherInfo afternoon = await getHourForecast(12);
			WeatherInfo evening   = await getHourForecast(18);

			string response = $"{city.Name}: Weer op {day}";
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