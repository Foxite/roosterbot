using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class MetaComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServices(IServiceCollection services, string configPath) {
			ResourcesType = typeof(Resources);

			return Task.CompletedTask;
		}

		public async override Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<HelpModule>(services),
				commandService.AddModuleAsync<ControlModule>(services)
			));
		}
	}
}
