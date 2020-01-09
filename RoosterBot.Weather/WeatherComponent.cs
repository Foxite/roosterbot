using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Weather {
	public class WeatherComponent : Component {
		public override Version ComponentVersion => new Version(0, 3, 0);
		public bool AttributionLicense { get; private set; }

		protected override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		protected async override Task AddServicesAsync(IServiceCollection services, string configPath) {
			Logger.Debug("Weather", "Loading cities file");
			var cityService = new CityService(configPath);
			await cityService.ReadCityCSVAsync();
			Logger.Debug("Weather", "Finished loading cities file");

			var jsonConfig = JsonConvert.DeserializeObject<WeatherJsonConfig>(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			services.AddSingleton(isp => new WeatherService(isp.GetService<ResourceService>(), jsonConfig.Key, jsonConfig.Attribution));
			services.AddSingleton(cityService);
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Weather.Resources");

			commandService.AddTypeParser(new CityInfoParser());

			commandService.AddModule<WeatherModule>();

			help.AddHelpSection(this, "#WeatherComponent_HelpName", "#WeatherComponent_HelpText");

			return Task.CompletedTask;
		}

		private class WeatherJsonConfig {
			public string Key { get; }
			public bool Attribution { get; }

			public WeatherJsonConfig(string weatherbit_key, bool attribution) {
				Key = weatherbit_key;
				Attribution = attribution;
			}
		}
	}
}
