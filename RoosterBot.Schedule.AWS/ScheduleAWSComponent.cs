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
		private DynamoDBUserClassesService? m_UserClasses;

		public override Version ComponentVersion => new Version(1, 0, 0);
		public override IEnumerable<string> Tags => new[] { "UserClassesService" };

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireMinimumVersion<ScheduleComponent>(new Version(2, 0, 0))
				.Check();
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			string tableName = jsonConfig["userClasses_tableName"].ToObject<string>();

			services.AddSingleton<IUserClassesService, DynamoDBUserClassesService>((isp) => {
				m_UserClasses = new DynamoDBUserClassesService(isp.GetService<AWSConfigService>(), tableName);
				return m_UserClasses;
			});

			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_UserClasses?.Dispose();
		}
	}
}
