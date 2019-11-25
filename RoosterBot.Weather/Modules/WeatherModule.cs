using System;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;
using Qmmands;

namespace RoosterBot.Weather {
	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("#WeatherModule_Name"), Group("#WeatherModule_Group"), LocalizedModule("nl-NL", "en-US")]
	public class WeatherModule : RoosterModuleBase {
		public WeatherService Weather { get; set; } = null!;

		[Command("#WeatherModule_CurrentWeather"), RunMode(RunMode.Parallel)]
		public async Task GetCurrentWeatherCommand([Remainder] CityInfo city) {
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				weather = await Weather.GetCurrentWeatherAsync(city);
			}
			GuildConfig.TryGetData("metric", out bool metric, true);
			ReplyDeferred(weather.Present(DateTime.Now, Culture, metric));
			Attribution();
		}

		[Command("#WeatherModule_DayForecast"), RunMode(RunMode.Parallel)]
		public async Task GetDayForecastCommand(DayOfWeek day, [Remainder] CityInfo city) {
			// Get the forecast for the day
			int daysFromNow = day - DateTime.Today.DayOfWeek;
			if (daysFromNow < 0) {
				daysFromNow += 7;
			}
			DateTime date = DateTime.Today.AddDays(daysFromNow);
			await RespondDayForecast(city, date);
			Attribution();
		}

		[Command("#WeatherModule_TimeForecast"), RunMode(RunMode.Parallel)]
		public async Task GetDayForecastCommand(DayOfWeek day, TimeSpan timeOffset, [Remainder] CityInfo city) {
			DateTime datetime;
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				// Get the forecast for the day at the time indicated by the DateTime object (the Date is ignored)
				datetime = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek).Add(timeOffset);
				weather = await Weather.GetWeatherForecastAsync(city, (int) (datetime - DateTime.Now).TotalHours);
			}
			GuildConfig.TryGetData("metric", out bool metric, true);
			ReplyDeferred(weather.Present(datetime, Culture, metric));
			Attribution();
		}

		[Command("#WeatherModule_UnitForecast"), RunMode(RunMode.Parallel)]
		public async Task GetForecastCommand(int amount, string unit, [Remainder] CityInfo city) {
			if (amount < 1) {
				MinorError(GetString("#WeatherModule_NoLookBack"));
			} else if (GetString("WeatherModule_Unit_Days").Split('|').Contains(unit)) {
				if (amount > 7) {
					MinorError(GetString("WeatherModule_SevenDayLimit"));
				} else {
					await RespondDayForecast(city, DateTime.Today.AddDays(amount));
					Attribution();
				}
			} else if (GetString("WeatherModule_Unit_Hours").Split('|').Contains(unit)) {
				if (amount > 168) {
					MinorError(GetString("WeatherModule_SevenDayLimit"));
				} else {
					WeatherInfo weather;
					using (IDisposable typingState = Context.Channel.EnterTypingState()) {
						weather = await Weather.GetWeatherForecastAsync(city, amount);
					}
					GuildConfig.TryGetData("metric", out bool metric, true);
					ReplyDeferred(weather.Present(DateTime.Now.AddHours(amount), Culture, metric));
					Attribution();
				}
			} else {
				MinorError(GetString("WeatherModule_UnknownUnit"));
			}
		}

		private async Task RespondDayForecast(CityInfo city, DateTime date) {
			string response;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				WeatherInfo[] dayForecast = await Weather.GetDayForecastAsync(city, date);

				string pretext;
				if (dayForecast[0].City.Name == dayForecast[0].City.Region.Name) {
					pretext = GetString("WeatherModule_DayForecast_PretextRegion", dayForecast[0].City.Name, dayForecast[0].City.Region.Name, DateTimeUtil.GetRelativeDateReference(date, Culture));
				} else {
					pretext = GetString("WeatherModule_DayForecast_PretextCity", dayForecast[0].City.Name, DateTimeUtil.GetRelativeDateReference(date, Culture));
				}

				GuildConfig.TryGetData("metric", out bool metric, true);

				response  = DateTime.Today.AddHours(08).ToShortTimeString(Culture) + "\n" + dayForecast[0].Present(Culture, metric);
				response += DateTime.Today.AddHours(12).ToShortTimeString(Culture) + "\n" + dayForecast[1].Present(Culture, metric);
				response += DateTime.Today.AddHours(18).ToShortTimeString(Culture) + "\n" + dayForecast[2].Present(Culture, metric);
			}

			ReplyDeferred(response);
		}

		private void Attribution() {
			if (Weather.Attribution) {
				ReplyDeferred(GetString("WeatherComponent_Attribution"));
			}
		}
	}
}
