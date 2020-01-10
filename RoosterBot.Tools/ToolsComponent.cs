using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace RoosterBot.Tools {
	public class ToolsComponent : Component {
		public override Version ComponentVersion => new Version(0, 2, 0);

		protected override Task AddServicesAsync(IServiceCollection services, string configPath) {
			var config = JsonConvert.DeserializeAnonymousType(File.ReadAllText(Path.Combine(configPath, "Config.json")), new {
				PathToFFMPEG = ""
			});

			services.AddSingleton<YoutubeClient>();
			services.AddSingleton((isp) => new YoutubeConverter(isp.GetService<YoutubeClient>(), config.PathToFFMPEG));

			return Task.CompletedTask;
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Tools.Resources");

			commandService.AddModule<YoutubeModule>();
			return Task.CompletedTask;
		}
	}
}
