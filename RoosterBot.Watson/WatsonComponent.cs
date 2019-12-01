using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Watson {
	public class WatsonComponent : Component {
		private string m_WatsonID = "";
		private string m_WatsonKey = "";

		public override Version ComponentVersion => new Version(1, 1, 0);
		public override ICollection<CultureInfo> SupportedCultures => new[] { CultureInfo.GetCultureInfo("nl-NL"), CultureInfo.GetCultureInfo("en-US") };

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_WatsonID = jsonConfig["watsonid"].ToObject<string>();
			m_WatsonKey = jsonConfig["watsonkey"].ToObject<string>();
			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			ResourceService resourceService = services.GetService<ResourceService>();
			resourceService.RegisterResources("RoosterBot.Watson.Resources");

			new WatsonHandler(
				services.GetService<DiscordSocketClient>(),
				services.GetService<UserConfigService>(),
				services.GetService<GuildConfigService>(),
				new WatsonClient(m_WatsonKey, m_WatsonID),
				commandService,
				resourceService);
			
			help.AddHelpSection(this, "#WatsonComponent_HelpName", "#WatsonComponent_HelpText");

			return Task.CompletedTask;
		}
	}
}
