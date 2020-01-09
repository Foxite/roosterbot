using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace RoosterBot.AWS {
	public class AWSComponent : Component {
		public override Version ComponentVersion => new Version(1, 2, 0);

#nullable disable
		private AmazonDynamoDBClient m_DynamoDBClient;
#nullable restore
		// This field may actually be null after startup, because it does not get created in debug builds. So don't exclude it from nullability.
		private SNSNotificationHandler? m_SNS;
		private string m_NotificationARN = "";

		protected override Task AddServicesAsync(IServiceCollection services, string configPath) {
			var jsonConfig = JsonConvert.DeserializeObject<JsonAWSConfig>(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			m_NotificationARN = jsonConfig.NotificationArn;

			var awsConfig = new AWSConfigService(jsonConfig.AccessKey, jsonConfig.SecretKey, RegionEndpoint.GetBySystemName(jsonConfig.Endpoint));
			services.AddSingleton(awsConfig);

			m_DynamoDBClient = new AmazonDynamoDBClient(awsConfig.Credentials, awsConfig.Region);

			services.AddSingleton<UserConfigService>(new DynamoDBUserConfigService(m_DynamoDBClient, jsonConfig.UserTable));
			services.AddSingleton<ChannelConfigService>((isp) => new DynamoDBGuildConfigService(isp.GetRequiredService<ConfigService>(), m_DynamoDBClient, jsonConfig.GuildTable));
			return Task.CompletedTask;
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			bool production = true;
			// This way, there will be no warnings about unused fields.
#if DEBUG
			production = false;
#endif
			if (production) {
				m_SNS = new SNSNotificationHandler(services.GetService<NotificationService>(), services.GetService<AWSConfigService>(), m_NotificationARN);
			}

			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_DynamoDBClient.Dispose();
			m_SNS?.Dispose();
		}

		private class JsonAWSConfig {
			public string AccessKey  { get; }
			public string SecretKey  { get; }
			public string NotificationArn    { get; }
			public string UserTable  { get; }
			public string GuildTable { get; }
			public string Endpoint   { get; }

			public JsonAWSConfig(string accessKey, string secretKey, string sns_arn, string userTable, string guildTable, string endpoint) {
				AccessKey = accessKey;
				SecretKey = secretKey;
				NotificationArn = sns_arn;
				UserTable = userTable;
				GuildTable = guildTable;
				Endpoint = endpoint;
			}
		}
	}
}
