using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	public class WeatherComponent : ComponentBase {
		public override Version ComponentVersion => new Version(0, 1, 0);

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		public async override Task AddServicesAsync(IServiceCollection services, string configPath) {
			CityService cityService = new CityService(configPath);
			await cityService.ReadCityCSVAsync();

			JObject jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			string weatherBitKey = jsonConfig["weatherbit_key"].ToObject<string>();

			services.AddSingleton(new WeatherService(weatherBitKey));
			services.AddSingleton(cityService);
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			commandService.AddTypeReader<CityInfo>(new CityInfoReader());

			registerModuleFunction(new[] {
				await commandService.AddModuleAsync<WeatherModule>(services)
			});
		}
	}
}
