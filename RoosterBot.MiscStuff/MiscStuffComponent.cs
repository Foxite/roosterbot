using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	public class MiscStuffComponent : ComponentBase {
		public string ConfigPath { get; private set; }

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			ResourcesType = typeof(Resources);

			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
			return Task.CompletedTask;
		}

		public override async Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<CounterModule>(services)
			));

			string helpText = Resources.MiscStuffComponent_HelpText;
			help.AddHelpSection("misc", helpText);
		}
	}
}
