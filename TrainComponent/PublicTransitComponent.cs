using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using PublicTransitComponent.Services;
using PublicTransitComponent.Modules;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

namespace PublicTransitComponent {
	public class PublicTransitComponent : ComponentBase {
		public override void AddServices(ref IServiceCollection services, string configPath) {
			#region Config
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			string username = jsonConfig["username"].ToObject<string>();
			string password = jsonConfig["password"].ToObject<string>();
			string defaultDepartureCode = jsonConfig["defaultDepartureCode"].ToObject<string>();
			#endregion Config

			HTTPClient http = new HTTPClient();
			XmlRestApi xml = new XmlRestApi(http.Client, "http://webservices.ns.nl/ns-api-", username, password);

			//services.AddSingleton(http);
			//services.AddSingleton(xml);
			services.AddSingleton(new NSAPI(xml));
			services.AddSingleton(new StationCodeService(Path.Combine(configPath, "stations.xml"), defaultDepartureCode));
		}
		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddModuleAsync<PTModule>(services);
			commandService.AddModuleAsync<StationsModule>(services);

			string helpText = "Met `!ov` kan je informatie opzoeken via de NS reisplanner.\n";
			helpText += "Dit ondersteunt alleen treinreizen, dus geen bussen. Ook kan je alleen treinstations in Nederland opzoeken, en geen steden, adressen, of andere plaatsen.\n";
			helpText += "Dit is hoe je de command gebruikt: `!ov <naam van vertrekstation>, <naam van aankomststation>`\n";
			helpText += "Als ik niet het goede station heb gevonden, kun een code invullen die je met `!stations` kan vinden. (Hieronder meer.) Gebruik dan `!ov $CODE`.\n";
			helpText += "Je kunt het vertrekstation overslaan. In dit geval wordt Utrecht Vaartsche Rijn gebruikt, want dit is om de hoek bij de school.\n";
			helpText += "De maker heeft een paar maanden geleden cooldowns uitgezet, maar als met !ov gespammed wordt, gaat de cooldown voor dat command op 5 seconden (vanwege API regels).\n\n";

			helpText += "Je kunt stations opzoeken met `!stations <naam van station>`";
			help.AddHelpSection("trein", helpText);
		}
	}
}
