using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : Component {
		public override Version ComponentVersion => new Version(1, 1, 0);
		public override ICollection<CultureInfo> SupportedCultures => new[] { CultureInfo.GetCultureInfo("nl-NL"), CultureInfo.GetCultureInfo("en-US") };

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			var jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			if (jsonConfig["useFileConfig"].ToObject<bool>()) {
				services.AddSingleton<GuildConfigService, FileGuildConfigService>(isp => new FileGuildConfigService(isp.GetRequiredService<ConfigService>(), Path.Combine(configPath, "Guilds.json")));
				services.AddSingleton<UserConfigService, FileUserConfigService>(isp => new FileUserConfigService(Path.Combine(configPath, "Users.json")));
			}

			services.AddSingleton(new MetaInfoService(jsonConfig["githubLink"].ToObject<string>(), jsonConfig["discordLink"].ToObject<string>()));

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			commandService.AddTypeParser(new CultureInfoParser(this));

			commandService.AddModule<CommandsListModule>();
			commandService.AddModule<HelpModule>();
			commandService.AddModule<ControlModule>();
			commandService.AddModule<GuildConfigModule>();
			commandService.AddModule<UserConfigModule>();
			commandService.AddModule<InfoModule>();

			help.AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");

			return Task.CompletedTask;
		}
	}
}
