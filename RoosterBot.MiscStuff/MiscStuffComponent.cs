using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	// TODO (refactor) Move stuff out of this component
	// This component is half composed of GLU-requested features, and half actual miscellanea.
	// The GLU stuff should go into Schedule.GLU, which should be renamed to GLU. It will contain all current and future GLU-specific stuff.
	public class MiscStuffComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 1, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;

		public override async Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.MiscStuff.Resources");

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<MiscModule>(services),
				commandService.AddModuleAsync<UserListModule>(services)
			));
		}
	}
}
