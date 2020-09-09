using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.PublicTransit {
	// Do not localize this component.
	// This functionality is only useful in the Netherlands.
	public class PublicTransitComponent : Component {
#nullable disable
		private NSAPI m_NSAPI;
#nullable restore

		public override Version ComponentVersion => new Version(1, 2, 0);

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Username = "",
				Password = "",
				DefaultDepartureCode = ""
			});

			m_NSAPI = new NSAPI(config.Username, config.Password);
			services.AddSingleton(m_NSAPI);
			services.AddSingleton(new StationInfoService(Path.Combine(configPath, "stations.xml"), config.DefaultDepartureCode));
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			var stationInfoReader = new StationInfoParser();
			commandService.AddTypeParser(stationInfoReader);
			commandService.AddTypeParser(new ArrayParser<StationInfo>(stationInfoReader));

			commandService.AddModule<PTModule>();
		}

		protected override void Dispose(bool disposing) {
			m_NSAPI?.Dispose();
		}
	}
}
