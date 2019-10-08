using System;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Weather {
	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("weer"), Group("weer")]
	public class WeatherModule : RoosterModuleBase {
		public WeatherService Weather { get; set; }

		[Command(RunMode = RunMode.Async), Alias("in")]
		public async Task GetCurrentWeatherCommand([Remainder] CityInfo city) {
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				weather = await Weather.GetCurrentWeatherAsync(city);
			}
			ReplyDeferred(weather.Present(DateTime.Now, Culture));
		}

		[Command(RunMode = RunMode.Async), Alias("op")]
		public async Task GetDayForecastCommand(DayOfWeek day, [Remainder] CityInfo city) {
			// Get the forecast for the day
			DateTime date = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek);
			await RespondDayForecast(city, date);
		}

		[Command(RunMode = RunMode.Async), Alias("op")]
		public async Task GetDayForecastCommand(DayOfWeek day, TimeSpan timeOffset, [Remainder] CityInfo city) {
			DateTime datetime;
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				// Get the forecast for the day at the time indicated by the DateTime object (the Date is ignored)
				datetime = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek).Add(timeOffset);
				weather = await Weather.GetWeatherForecastAsync(city, (int) (datetime - DateTime.Now).TotalHours);
			}
			ReplyDeferred(weather.Present(datetime, Culture));
		}

		[Command("over", RunMode = RunMode.Async)]
		public async Task GetForecastCommand(int amount, string unit, [Remainder] CityInfo city) {
			if (amount < 1) {
				await MinorError("Ik kan niet terug kijken.");
			} else if (unit == "dag" || unit == "dagen") {
				if (amount > 7) {
					await MinorError("Ik kan het weer niet verder dan 7 dagen voorspellen.");
				} else {
					await RespondDayForecast(city, DateTime.Today.AddDays(amount));
				}
			} else if (unit == "uur") {
				if (amount > 168) {
					await MinorError("Ik kan het weer niet verder dan 7 dagen voorspellen.");
				} else {
					WeatherInfo weather;
					using (IDisposable typingState = Context.Channel.EnterTypingState()) {
						weather = await Weather.GetWeatherForecastAsync(city, amount);
					}
					ReplyDeferred(weather.Present(DateTime.Now.AddHours(amount), Culture));
				}
			} else {
				await MinorError("Ik kan alleen op uur- of dagniveau het weer voorspellen.");
			}
		}

		private async Task RespondDayForecast(CityInfo city, DateTime date) {
			string response;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				WeatherInfo[] dayForecast = await Weather.GetDayForecastAsync(city, date);

				response = $"{dayForecast[0].City.Name}, {dayForecast[0].City.Region}: Weer {DateTimeUtil.GetRelativeDateReference(date, Culture)}\n";
				response += "08:00:\n" + dayForecast[0].Present();
				response += "\n\n12:00:\n" + dayForecast[1].Present();
				response += "\n\n18:00:\n" + dayForecast[2].Present();
			}

			ReplyDeferred(response);
		}
	}
}
