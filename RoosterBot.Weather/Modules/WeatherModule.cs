using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Weather {
	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("#WeatherModule_Name"), Group("#WeatherModule_Group"), LocalizedModule("nl-NL", "en-US")]
	public class WeatherModule : RoosterModuleBase {
		public WeatherService Weather { get; set; }

		[Command("#WeatherModule_CurrentWeather", RunMode = RunMode.Async)]
		public async Task GetCurrentWeatherCommand([Remainder] CityInfo city) {
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				weather = await Weather.GetCurrentWeatherAsync(city);
			}
			ReplyDeferred(weather.Present(DateTime.Now, Culture, true)); // TODO (feature) guild config for metric (probably use a dynamic config system so any component can have their own settings)
			Attribution();
		}

		[Command("#WeatherModule_DayForecast", RunMode = RunMode.Async)]
		public async Task GetDayForecastCommand(DayOfWeek day, [Remainder] CityInfo city) {
			// Get the forecast for the day
			DateTime date = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek);
			await RespondDayForecast(city, date);
			Attribution();
		}

		[Command("#WeatherModule_TimeForecast", RunMode = RunMode.Async)]
		public async Task GetDayForecastCommand(DayOfWeek day, TimeSpan timeOffset, [Remainder] CityInfo city) {
			DateTime datetime;
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				// Get the forecast for the day at the time indicated by the DateTime object (the Date is ignored)
				datetime = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek).Add(timeOffset);
				weather = await Weather.GetWeatherForecastAsync(city, (int) (datetime - DateTime.Now).TotalHours);
			}
			ReplyDeferred(weather.Present(datetime, Culture, true));
			Attribution();
		}

		[Command("#WeatherModule_UnitForecast", RunMode = RunMode.Async)]
		public async Task GetForecastCommand(int amount, string unit, [Remainder] CityInfo city) {
			if (amount < 1) {
				await MinorError(GetString("#WeatherModule_NoLookBack"));
			} else if (GetString("WeatherModule_Unit_Days").Split('|').Contains(unit)) {
				if (amount > 7) {
					await MinorError(GetString("WeatherModule_SevenDayLimit"));
				} else {
					await RespondDayForecast(city, DateTime.Today.AddDays(amount));
					Attribution();
				}
			} else if (GetString("WeatherModule_Unit_Hours").Split('|').Contains(unit)) {
				if (amount > 168) {
					await MinorError(GetString("WeatherModule_SevenDayLimit"));
				} else {
					WeatherInfo weather;
					using (IDisposable typingState = Context.Channel.EnterTypingState()) {
						weather = await Weather.GetWeatherForecastAsync(city, amount);
					}
					ReplyDeferred(weather.Present(DateTime.Now.AddHours(amount), Culture, true));
					Attribution();
				}
			} else {
				await MinorError(GetString("WeatherModule_UnknownUnit"));
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
				
				response  = DateTime.Today.AddHours(08).ToShortTimeString(Culture) + "\n" + dayForecast[0].Present(Culture, true);
				response += DateTime.Today.AddHours(12).ToShortTimeString(Culture) + "\n" + dayForecast[1].Present(Culture, true);
				response += DateTime.Today.AddHours(18).ToShortTimeString(Culture) + "\n" + dayForecast[2].Present(Culture, true);
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
