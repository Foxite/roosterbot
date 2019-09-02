﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Schedule.AWS {
	public class ScheduleAWSComponent : ComponentBase {
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

			string         dbKeyId     = jsonConfig["ucdb"]["keyId"    ].ToObject<string>();
			string	       dbSecretKey = jsonConfig["ucdb"]["secretKey"].ToObject<string>();
			string         tableName   = jsonConfig["ucdb"]["tableName"].ToObject<string>();
			RegionEndpoint dbEndpoint  = RegionEndpoint.GetBySystemName(jsonConfig["ucdb"]["endpoint"].ToObject<string>());
			m_UserClasses = new DynamoDBUserClassesService(dbKeyId, dbSecretKey, dbEndpoint, tableName);

			services.AddSingleton(typeof(IUserClassesService), m_UserClasses);

			return Task.CompletedTask;
		}

		public override Task ShutdownAsync() {
			m_UserClasses.Dispose();
			return Task.CompletedTask;
		}
	}
}
