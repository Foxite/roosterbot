using Discord.Commands;
using RoosterBot.DateTimeUtils;
using System;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	// TODO localize? The API is not restricted to the Netherlands but we would have to provide the entire cities file, which takes considerable time to read
	// Alternatively we could provide a cities file with only the cities in countries we are localized to

	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("weer"), Group("weer")]
	public class WeatherModule : RoosterModuleBase {
		public WeatherService Weather { get; set; }

		[Command(RunMode = RunMode.Async), Alias("nu")]
		public async Task GetCurrentWeatherCommand(CityInfo city) {
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				weather = await Weather.GetCurrentWeatherAsync(city);
			}
			ReplyDeferred(weather.Present());
		}

		[Command(RunMode = RunMode.Async), Alias("dag")]
		public async Task GetDayForecastCommand(DayOfWeek day, CityInfo city) {
			// Get the forecast for the day at {hours} o clock
			DateTime date = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek);
			await RespondDayForecast(city, date);
		}

		[Command("over", RunMode = RunMode.Async)]
		public async Task GetForecastCommand(int amount, string unit, CityInfo city) {
			if (unit == "dag" || unit == "dagen") {
				if (amount > 7) {
					await MinorError("Ik kan het weer niet verder dan 7 dagen voorspellen.");
				} else {
					await RespondDayForecast(city, DateTime.Today.AddDays(amount));
				}
			} else if (unit == "uur") {
				WeatherInfo weather;
				using (IDisposable typingState = Context.Channel.EnterTypingState()) {
					weather = await Weather.GetWeatherForecastAsync(city, amount);
				}
				ReplyDeferred(weather.Present());
			} else {
				await MinorError("Ik kan alleen op uur- of dagniveau het weer voorspellen.");
			}
		}

		private async Task RespondDayForecast(CityInfo city, DateTime date) {
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				WeatherInfo[] dayForecast = await Weather.GetDayForecastAsync(city, date);

				// Copied from ScheduleModule, should probably move into DateTimeUtil, although the fact that the strings need to be localized is annoying
				// DateTimeUtils is not component and can't make use of RoosterBot's resource system, but there's no reason we *can't* make it one
				string relativeDateReference;
				if (date == DateTime.Today) {
					relativeDateReference = "vandaag";
				} else if (date == DateTime.Today.AddDays(1)) {
					relativeDateReference = "morgen";
				} else if ((date - DateTime.Today).TotalDays < 7) {
					relativeDateReference = "op " + date.DayOfWeek.GetName(Culture);
				} else {
					relativeDateReference = "op " + date.ToShortDateString(Culture);
				}

				string response = $"{city.Name}: Weer {relativeDateReference}\n";
				response += "08:00: " + dayForecast[0].Present();
				response += "\n12:00: " + dayForecast[1].Present();
				response += "\n18:00: " + dayForecast[2].Present();

				ReplyDeferred(response);
			}
		}
	}
}
