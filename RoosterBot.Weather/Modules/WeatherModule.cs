using System;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;
using Qmmands;

namespace RoosterBot.Weather {
	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("#WeatherModule_Name"), Group("#WeatherModule_Group")]
	public class WeatherModule : RoosterModule {
		private readonly CompoundResult m_Result = new CompoundResult("\n");

		public WeatherService Weather { get; set; } = null!;

		[Command("#WeatherModule_CurrentWeather")]
		public async Task<CommandResult> GetCurrentWeatherCommand([Remainder] CityInfo city) {
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				weather = await Weather.GetCurrentWeatherAsync(city);
			}
			GuildConfig.TryGetData("metric", out bool metric, true);
			m_Result.AddResult(weather.Present(DateTime.Now, Culture, metric));
			Attribution();
			return m_Result;
		}

		[Command("#WeatherModule_DayForecast")]
		public async Task<CommandResult> GetDayForecastCommand(DayOfWeek day, [Remainder] CityInfo city) {
			// Get the forecast for the day
			int daysFromNow = day - DateTime.Today.DayOfWeek;
			if (daysFromNow < 0) {
				daysFromNow += 7;
			}
			DateTime date = DateTime.Today.AddDays(daysFromNow);
			await RespondDayForecast(city, date);
			Attribution();
			return m_Result;
		}

		[Command("#WeatherModule_TimeForecast")]
		public async Task<CommandResult> GetDayForecastCommand(DayOfWeek day, TimeSpan timeOffset, [Remainder] CityInfo city) {
			DateTime datetime;
			WeatherInfo weather;
			using (IDisposable typingState = Context.Channel.EnterTypingState()) {
				// Get the forecast for the day at the time indicated by the DateTime object (the Date is ignored)
				datetime = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek).Add(timeOffset);
				weather = await Weather.GetWeatherForecastAsync(city, (int) (datetime - DateTime.Now).TotalHours);
			}
			GuildConfig.TryGetData("metric", out bool metric, true);
			m_Result.AddResult(weather.Present(datetime, Culture, metric));
			Attribution();
			return m_Result;
		}

		[Command("#WeatherModule_UnitForecast")]
		public async Task<CommandResult> GetForecastCommand(int amount, string unit, [Remainder] CityInfo city) {
			if (amount < 1) {
				return TextResult.Error(GetString("#WeatherModule_NoLookBack"));
			} else if (GetString("WeatherModule_Unit_Days").Split('|').Contains(unit)) {
				if (amount > 7) {
					return TextResult.Error(GetString("WeatherModule_SevenDayLimit"));
				} else {
					await RespondDayForecast(city, DateTime.Today.AddDays(amount));
					Attribution();
					return m_Result;
				}
			} else if (GetString("WeatherModule_Unit_Hours").Split('|').Contains(unit)) {
				if (amount > 168) {
					return TextResult.Error(GetString("WeatherModule_SevenDayLimit"));
				} else {
					WeatherInfo weather;
					using (IDisposable typingState = Context.Channel.EnterTypingState()) {
						weather = await Weather.GetWeatherForecastAsync(city, amount);
					}
					GuildConfig.TryGetData("metric", out bool metric, true);
					m_Result.AddResult(weather.Present(DateTime.Now.AddHours(amount), Culture, metric));
					Attribution();
					return m_Result;
				}
			} else {
				return TextResult.Error(GetString("WeatherModule_UnknownUnit"));
			}
		}

		private async Task RespondDayForecast(CityInfo city, DateTime date) {
			using IDisposable typingState = Context.Channel.EnterTypingState();

			WeatherInfo[] dayForecast = await Weather.GetDayForecastAsync(city, date);

			string pretext;
			if (dayForecast[0].City.Name == dayForecast[0].City.Region.Name) {
				pretext = GetString("WeatherModule_DayForecast_PretextRegion", dayForecast[0].City.Name, dayForecast[0].City.Region.Name, DateTimeUtil.GetRelativeDateReference(date, Culture));
			} else {
				pretext = GetString("WeatherModule_DayForecast_PretextCity", dayForecast[0].City.Name, DateTimeUtil.GetRelativeDateReference(date, Culture));
			}
			m_Result.AddResult(new TextResult(null, pretext));

			GuildConfig.TryGetData("metric", out bool metric, true);

			void addItem(int hours, int item) {
				m_Result.AddResult(dayForecast[item].Present(DateTime.Today.AddHours(hours).ToString("s", Culture), Culture, metric));
			}

			addItem(08, 0);
			addItem(12, 1);
			addItem(18, 2);
		}

		private void Attribution() {
			if (Weather.Attribution) {
				m_Result.AddResult(new TextResult(null, GetString("WeatherComponent_Attribution")));
			}
		}
	}
}
