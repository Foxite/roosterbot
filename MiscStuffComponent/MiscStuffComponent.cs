using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));

			commandService.AddModuleAsync<CounterModule>();
			commandService.AddModuleAsync<MiscModule>();
		}
	}
}
