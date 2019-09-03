using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot.AWS;

namespace RoosterBot.Schedule.AWS {
	public class ScheduleAWSComponent : ComponentBase {
		private string m_TableName;
		private DynamoDBUserClassesService m_UserClasses;

		public override Version ComponentVersion => new Version(1, 0, 0);
		public override string[] Tags => new[] { "UserClassesService" };

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireMinimumVersion<ScheduleComponent>(new Version(2, 0, 0))
				.Check();
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_TableName = jsonConfig["userClasses_tableName"].ToObject<string>();
			m_UserClasses = new DynamoDBUserClassesService();

			services.AddSingleton(typeof(IUserClassesService), m_UserClasses);

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			m_UserClasses.Initialize(services.GetService<AWSConfigService>(), m_TableName);

			return Task.CompletedTask;
		}

		public override Task ShutdownAsync() {
			m_UserClasses.Dispose();
			return Task.CompletedTask;
		}
	}
}
