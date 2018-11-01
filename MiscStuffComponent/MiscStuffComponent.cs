using System.IO;
using RoosterBot;
using Newtonsoft.Json.Linq;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "Counters")));

			commandService.AddModuleAsync<CounterModule>();
		}
	}
}
