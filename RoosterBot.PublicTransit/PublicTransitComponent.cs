using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Threading.Tasks;

namespace RoosterBot.PublicTransit {
	// Do not localize this component.
	// This functionality is only useful in the Netherlands.
	public class PublicTransitComponent : Component {
#nullable disable
		private NSAPI m_NSAPI;
#nullable restore

		public override Version ComponentVersion => new Version(1, 1, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			#region Config
			var jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));
			string username = jsonConfig["username"].ToObject<string>();
			string password = jsonConfig["password"].ToObject<string>();
			string defaultDepartureCode = jsonConfig["defaultDepartureCode"].ToObject<string>();
			#endregion Config

			m_NSAPI = new NSAPI(username, password);
			services.AddSingleton(m_NSAPI);
			services.AddSingleton(new StationInfoService(Path.Combine(configPath, "stations.xml"), defaultDepartureCode));

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			var stationInfoReader = new StationInfoParser();
			commandService.AddTypeParser(stationInfoReader);
			commandService.AddTypeParser(new ArrayParser<StationInfo>(stationInfoReader));

			commandService.AddModule<PTModule>();

			string helpText = "Met `{0}ov` kan je informatie opzoeken via de NS reisplanner.\n";
			helpText += "Dit ondersteunt alleen treinreizen, dus geen bussen. Ook kan je alleen treinstations in Nederland opzoeken, en geen steden, adressen, of andere plaatsen.\n";
			helpText += "Dit is hoe je de command gebruikt: `{0}ov <naam van vertrekstation>, <naam van aankomststation>`\n";
			helpText += "Als ik niet het goede station heb gevonden, kun een code invullen die je met `{0}stations` kan vinden. (Hieronder meer.) Gebruik dan `{0}ov $CODE`.\n";
			helpText += "Je kunt het vertrekstation overslaan. In dit geval wordt Utrecht Vaartsche Rijn gebruikt, want dit is om de hoek bij de school.\n\n";

			helpText += "Je kunt stations opzoeken met `{0}stations <naam van station>`";
			help.AddHelpSection(this, "trein", string.Format(helpText, services.GetService<ConfigService>().DefaultCommandPrefix));

			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_NSAPI?.Dispose();
		}
	}
}
