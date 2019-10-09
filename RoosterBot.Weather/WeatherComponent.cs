using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Weather {
	// TODO localize the entire component
	public class WeatherComponent : ComponentBase {
		public override Version ComponentVersion => new Version(0, 1, 0);

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
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

			services.AddSingleton(new WeatherService(weatherBitKey));
			services.AddSingleton(cityService);
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			commandService.AddTypeReader<CityInfo>(new CityInfoReader());

			registerModuleFunction(new[] {
				await commandService.AddModuleAsync<WeatherModule>(services)
			});

			string helpText = "Ik kan weersvoorspellingen voor je opzoeken en in Discord laten zien. Hiervoor gebruik je het `weer` command.\n";
			helpText += "Deze command heeft een aantal verschillende vormen:\n\n";
			helpText += "`{0}weer in utrecht` laat de huidige weersomstandigheden in Utrecht zien. (`{0}weer nu utrecht` of `{0}weer utrecht` werkt ook.)\n";
			helpText += "`{0}weer over 2 uur amsterdam` of `{0}weer op woensdag 15:00 rotterdam`: Deze voorbeelden spreken voor zich.\n";
			helpText += "`{0}weer over 3 dagen hilversum` of `{0}weer op woensdag utrecht`: Deze commands laten het weer zien op de aangegeven dag, om 08:00, 12:00, en 18:00 uur.\n\n";
			helpText += "Enkele steden (Den Bosch en Den Haag) staan bekend onder alternatieve spellingen. Als je een stad tegenkomt waarvan de alternatieve spelling niet bekend is, laat dit weten aan de bot eigenaar.";


			help.AddHelpSection(this, "weer", string.Format(helpText, services.GetService<ConfigService>().DefaultCommandPrefix));
		}
	}
}
