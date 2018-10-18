using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using TrainComponent.Services;
using TrainComponent.Modules;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TrainComponent {
	public class PublicTransitComponent : ComponentBase {
		public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
			#region Config
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			string username = jsonConfig["nsapi_Username"].ToObject<string>();
			string password = jsonConfig["nsapi_Password"].ToObject<string>();
			#endregion Config
			
			services.AddSingleton(new XmlRestApi("http://webservices.ns.nl/ns-api-", username, password));

			commandService.AddModuleAsync<PTModule>();
		}
	}
}
