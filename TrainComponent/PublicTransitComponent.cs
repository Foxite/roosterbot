using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using PublicTransitComponent.Services;
using PublicTransitComponent.Modules;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PublicTransitComponent {
	public class PublicTransitComponent : ComponentBase {
		public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
			#region Config
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			string username = jsonConfig["username"].ToObject<string>();
			string password = jsonConfig["password"].ToObject<string>();
			#endregion Config

			HTTPClient http = new HTTPClient();
			XmlRestApi xml = new XmlRestApi(http.Client, "http://webservices.ns.nl/ns-api-", username, password);

			//services.AddSingleton(http);
			//services.AddSingleton(xml);
			services.AddSingleton(new NSAPI(xml));

			commandService.AddModuleAsync<PTModule>();
		}
	}
}
