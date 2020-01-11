using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace RoosterBot.Tools {
	public class ToolsComponent : Component {
		public override Version ComponentVersion => new Version(0, 3, 0);

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				PathToFFMPEG = ""
			});

			services.AddSingleton<YoutubeClient>();
			services.AddSingleton((isp) => new YoutubeConverter(isp.GetService<YoutubeClient>(), config.PathToFFMPEG));
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Tools.Resources");

			commandService.AddModule<YoutubeModule>();
		}
	}
}
