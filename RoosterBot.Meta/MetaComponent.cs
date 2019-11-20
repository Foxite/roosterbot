using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 1, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			JObject jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			if (jsonConfig["useFileConfig"].ToObject<bool>()) {
				services.AddSingleton<GuildConfigService, FileGuildConfigService>(isp => new FileGuildConfigService(isp.GetRequiredService<ConfigService>(), Path.Combine(configPath, "Guilds.json")));
				services.AddSingleton<UserConfigService, FileUserConfigService>(isp => new FileUserConfigService(Path.Combine(configPath, "Users.json")));
			}

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			commandService.AddTypeReader<CultureInfo>(new CultureInfoReader());

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<HelpModule>(services),
				commandService.AddModuleAsync<ControlModule>(services),
				commandService.AddModuleAsync<InfoModule>(services)
			));

			registerModules(await commandService.AddLocalizedModuleAsync<GuildConfigModule>());
			registerModules(await commandService.AddLocalizedModuleAsync<UserConfigModule>());

			help.AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");
		}
	}
}
