using System;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.PublicTransit {
	// Do not localize this component.
	// This functionality is only useful in the Netherlands.
	public class PublicTransitComponent : Component {
		internal const string LogTag = "PublicTransit";

		public override Version ComponentVersion => new Version(1, 2, 1);

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Key = "",
				DefaultDepartureCode = ""
			});

			services.AddSingleton(isp => new NSAPI(config.Key, isp.GetRequiredService<HttpClient>()));
			services.AddSingleton(new StationInfoService(Path.Combine(configPath, "stations.xml"), config.DefaultDepartureCode));
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			var stationInfoReader = new StationInfoParser();
			commandService.AddTypeParser(stationInfoReader);
			commandService.AddTypeParser(new ArrayParser<StationInfo>(stationInfoReader));

			commandService.AddAllModules();
		}
	}
}
