using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public static string DiscordServerInvite { get; private set; }

		private string m_ConfigPath;

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			m_ConfigPath = configPath;

			DiscordServerInvite = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")))["discordServerInvite"].ToObject<string>();

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");
			services.GetService<GuildConfigService>().InstallProvider(new FileGuildConfigProvider(Path.Combine(m_ConfigPath, "GuildConfig.json")));

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<HelpModule>(services),
				commandService.AddModuleAsync<ControlModule>(services),
				commandService.AddModuleAsync<DiagnosticModule>(services),
				commandService.AddModuleAsync<InfoModule>(services)
			));

			help.AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");
		}
	}
}
