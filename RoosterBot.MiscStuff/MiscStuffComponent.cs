using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	// TODO (refactor) Move stuff out of this component
	// This component is half composed of GLU-requested features, and half actual miscellanea.
	// The GLU stuff should go into Schedule.GLU, which should be renamed to GLU. It will contain all current and future GLU-specific stuff.
	public class MiscStuffComponent : ComponentBase {
#nullable disable
		public string ConfigPath { get; private set; }
#nullable restore

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
			services.AddSingleton<PrankService>();
			return Task.CompletedTask;
		}

		public override async Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.MiscStuff.Resources");

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<MiscModule>(services),
				commandService.AddModuleAsync<CounterModule>(services),
				commandService.AddModuleAsync<ModerationModule>(services)
			));
		}
	}
}
