using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(configPath));

			commandService.AddModuleAsync<CounterModule>();
			commandService.AddModuleAsync<MiscModule>();
		}
	}
}
