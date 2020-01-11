using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Weather {
	public class WeatherComponent : Component {
		public override Version ComponentVersion => new Version(0, 3, 0);
		public bool AttributionLicense { get; private set; }

		protected override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			Logger.Debug("Weather", "Loading cities file");
			var cityService = new CityService(configPath);
			cityService.ReadCityCSV();
			Logger.Debug("Weather", "Finished loading cities file");

			var jsonConfig = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				WeatherBitKey = "",
				Attribution = false
			});

			services.AddSingleton(isp => new WeatherService(isp.GetService<ResourceService>(), jsonConfig.WeatherBitKey, jsonConfig.Attribution));
			services.AddSingleton(cityService);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Weather.Resources");

			commandService.AddTypeParser(new CityInfoParser());

			commandService.AddModule<WeatherModule>();

			help.AddHelpSection(this, "#WeatherComponent_HelpName", "#WeatherComponent_HelpText");
		}
	}
}
