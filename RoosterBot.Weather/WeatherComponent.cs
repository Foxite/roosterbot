﻿using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class WeatherComponent : Component {
		public override Version ComponentVersion => new Version(0, 3, 0);
		public override IReadOnlyCollection<CultureInfo> SupportedCultures => new[] { CultureInfo.GetCultureInfo("nl-NL"), CultureInfo.GetCultureInfo("en-US") };
		public bool AttributionLicense { get; private set; }

		public override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		public async override Task AddServicesAsync(IServiceCollection services, string configPath) {
			Logger.Debug("Weather", "Loading cities file");
			CityService cityService = new CityService(configPath);
			await cityService.ReadCityCSVAsync();
			Logger.Debug("Weather", "Finished loading cities file");

			JObject jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			string weatherBitKey = jsonConfig["weatherbit_key"].ToObject<string>();
			bool attributionLicense = jsonConfig["attribution"].ToObject<bool>();

			services.AddSingleton(provider => { // Do it like this because we need a dependency service, but we can't access those yet
				return new WeatherService(provider.GetService<ResourceService>(), weatherBitKey, attributionLicense);
			});
			services.AddSingleton(cityService);
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Weather.Resources");

			commandService.AddTypeParser(new CityInfoParser(this));

			commandService.AddModule<WeatherModule>();

			help.AddHelpSection(this, "#WeatherComponent_HelpName", "#WeatherComponent_HelpText");

			return Task.CompletedTask;
		}
	}
}
