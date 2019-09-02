using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.PublicTransit {
	// TODO prevent its use outside specific (Dutch) servers, do not localize
	// This functionality is only useful in the Netherlands.
	public class PublicTransitComponent : ComponentBase {
		private NSAPI m_NSAPI;

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			#region Config
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			string username = jsonConfig["username"].ToObject<string>();
			string password = jsonConfig["password"].ToObject<string>();
			string defaultDepartureCode = jsonConfig["defaultDepartureCode"].ToObject<string>();
			#endregion Config

			m_NSAPI = new NSAPI(username, password);
			services.AddSingleton(m_NSAPI);
			services.AddSingleton(new StationCodeService(Path.Combine(configPath, "stations.xml"), defaultDepartureCode));

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			registerModules(new[] { await commandService.AddModuleAsync<PTModule>(services) });

			string helpText = "Met `!ov` kan je informatie opzoeken via de NS reisplanner.\n";
			helpText += "Dit ondersteunt alleen treinreizen, dus geen bussen. Ook kan je alleen treinstations in Nederland opzoeken, en geen steden, adressen, of andere plaatsen.\n";
			helpText += "Dit is hoe je de command gebruikt: `!ov <naam van vertrekstation>, <naam van aankomststation>`\n";
			helpText += "Als ik niet het goede station heb gevonden, kun een code invullen die je met `!stations` kan vinden. (Hieronder meer.) Gebruik dan `!ov $CODE`.\n";
			helpText += "Je kunt het vertrekstation overslaan. In dit geval wordt Utrecht Vaartsche Rijn gebruikt, want dit is om de hoek bij de school.";

			helpText += "Je kunt stations opzoeken met `!stations <naam van station>`";
			help.AddHelpSection("trein", helpText);
		}

		public override Task ShutdownAsync() {
			m_NSAPI.Dispose();

			return Task.CompletedTask;
		}
	}
}
