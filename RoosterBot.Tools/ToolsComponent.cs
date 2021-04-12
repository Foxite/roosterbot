using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;

namespace RoosterBot.Tools {
	public class ToolsComponent : Component {
		public static ToolsComponent Instance { get; private set; } = null!;

		private InspirobotProvider? m_Inspirobot;

		public override Version ComponentVersion => new Version(1, 2, 0);
		public string PathToFFMPEG { get; private set; } = null!;

		public ToolsComponent() {
			Instance = this;
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				PathToFFMPEG = "",
				MotivationProviders = Array.Empty<string>()
			});

			PathToFFMPEG = config.PathToFFMPEG;

			services.AddSingleton<YoutubeClient>();

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

			commandService.AddModule<RunescapeModule>();
			commandService.AddModule<MotivationModule>();
			commandService.AddModule<StrawpollModule>();
			commandService.AddModule<YoutubeModule>();
			commandService.AddModule<PingModule>();
		}

		protected override void Dispose(bool disposing) {
			if (disposing && m_Inspirobot != null) {
				m_Inspirobot.Dispose();
			}
		}
	}
}
