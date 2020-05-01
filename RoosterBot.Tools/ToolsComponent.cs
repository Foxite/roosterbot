using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace RoosterBot.Tools {
	public class ToolsComponent : Component {
		private InspirobotProvider? m_Inspirobot;

		public override Version ComponentVersion => new Version(1, 0, 0);

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				PathToFFMPEG = "",
				MotivationProviders = new List<string>()
			});

			services.AddSingleton<YoutubeClient>();
			services.AddSingleton(isp => new YoutubeConverter(isp.GetRequiredService<YoutubeClient>(), config.PathToFFMPEG));

			services.AddSingleton(isp => {
				var ret = new MotivationService(isp.GetRequiredService<Random>());

				foreach (string provider in config.MotivationProviders) {
					switch (provider) {
						case "inspirobot":
							m_Inspirobot = new InspirobotProvider();
							ret.AddProvider(m_Inspirobot);
							break;
							// Add more providers here
						default:
							Logger.Warning("Tools", $"Unrecognized motivation provider: {provider}");
							break;
					}
				}

				return ret;
			});
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.Tools.Resources");

			commandService.AddModule<MotivationModule>();
			commandService.AddModule<StrawpollModule>();
			commandService.AddModule<YoutubeModule>();
		}

		protected override void Dispose(bool disposing) {
			if (disposing && m_Inspirobot != null) {
				m_Inspirobot.Dispose();
			}
		}
	}
}
