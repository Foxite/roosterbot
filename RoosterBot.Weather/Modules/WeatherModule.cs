﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Weather {
	// The free license for Weatherbit allows 500 calls per month, so we shouldn't try to show too much data at once. If the user wants to know the weather 3 hours from now, they
	// should request just that, instead of being shown a per-hour forecast of the entire day (24 calls), while they only care about one data point.
	[Name("#WeatherModule_Name"), Group("#WeatherModule_Group")]
	public class WeatherModule : RoosterModule {
		private readonly CompoundResult m_Result = new CompoundResult("\n");

		public WeatherService Weather { get; set; } = null!;

		[Command("#WeatherModule_CurrentWeather")]
		public async Task<CommandResult> GetCurrentWeatherCommand([Name("#WeatherModule_CityInfo_Name"), Remainder] CityInfo city) {
			using IDisposable typingState = Context.Channel.EnterTypingState();

			WeatherInfo weather;

			weather = await Weather.GetCurrentWeatherAsync(city);
			GuildConfig.TryGetData("metric", out bool metric, true);
			m_Result.AddResult(weather.Present(DateTime.Now, Culture, metric));
			Attribution();

			return m_Result;
		}

		[Command("#WeatherModule_TimeForecast")]
		public async Task<CommandResult> GetDayForecastCommand([Name("#WeatherModule_Forecast_Day")] DayOfWeek day, [Name("#WeatherModule_Forecast_Time")] TimeSpan timeOffset, [Name("#WeatherModule_CityInfo_Name"), Remainder] CityInfo city) {
			using IDisposable typingState = Context.Channel.EnterTypingState();

			DateTime datetime;
			WeatherInfo weather;

			// Get the forecast for the day at the time indicated by the DateTime object (the Date is ignored)
			datetime = DateTime.Today.AddDays(day - DateTime.Today.DayOfWeek).Add(timeOffset);
			weather = await Weather.GetWeatherForecastAsync(city, (int) (datetime - DateTime.Now).TotalHours);
			GuildConfig.TryGetData("metric", out bool metric, true);
			m_Result.AddResult(weather.Present(datetime, Culture, metric));
			Attribution();

			return m_Result;
		}

		[Command("#WeatherModule_UnitForecast")]
		public async Task<CommandResult> GetForecastCommand([Name("#WeatherModule_Forecast_Amount")] int amount, [GrammarParameter, Name("#WeatherModule_Forecast_Hours")] string unit, [Name("#WeatherModule_CityInfo_Name"), Remainder] CityInfo city) {
			if (amount < 1) {
				return TextResult.Error(GetString("#WeatherModule_NoLookBack"));
			} else if (GetString("WeatherModule_Unit_Hours").Split('|').Contains(unit)) {
				if (amount > 48) {
					return TextResult.Error(GetString("WeatherModule_TwoDayLimit"));
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

		private void Attribution() {
			if (Weather.Attribution) {
				m_Result.AddResult(new TextResult(null, GetString("WeatherComponent_Attribution")));
			}
		}
	}
}
