using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Schedule.AWS {
	public class ScheduleAWSComponent : ComponentBase {
		private DynamoDBUserClassesService m_UserClasses;

		public override Version ComponentVersion => new Version(0, 1, 0);

		public override Task AddServices(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			string         dbKeyId     = jsonConfig["ucdb"]["keyId"    ].ToObject<string>();
			string	       dbSecretKey = jsonConfig["ucdb"]["secretKey"].ToObject<string>();
			string         tableName   = jsonConfig["ucdb"]["tableName"].ToObject<string>();
			RegionEndpoint dbEndpoint  = RegionEndpoint.GetBySystemName(jsonConfig["ucdb"]["endpoint"].ToObject<string>());
			m_UserClasses = new DynamoDBUserClassesService(dbKeyId, dbSecretKey, dbEndpoint, tableName);

			services.AddSingleton(m_UserClasses);

			return Task.CompletedTask;
		}

		public override Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			return Task.CompletedTask;
		}

		public override Task OnShutdown() {
			m_UserClasses.Dispose();
			return Task.CompletedTask;
		}
	}
}
