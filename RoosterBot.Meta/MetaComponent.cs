using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class MetaComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<HelpModule>(services),
				commandService.AddModuleAsync<ControlModule>(services)
			));
		}
	}
}
