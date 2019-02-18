using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;
using System;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override void AddServices(ref IServiceCollection services, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService) {
			commandService.AddModuleAsync<CounterModule>(services);
			commandService.AddModuleAsync<MiscModule>(services);
		}
	}
}
