#if DEBUG
#pragma warning disable IDE0052 // Private member assigned but never used
#pragma warning disable CS0649 // Field never assigned to
#endif

using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.AWS {
	public class AWSComponent : Component {
		public override Version ComponentVersion => new Version(1, 2, 0);

#nullable disable
		private AmazonDynamoDBClient m_DynamoDBClient;
#nullable restore
		// This field may actually be null after startup, because it does not get created in debug builds. So don't exclude it from nullability.
		private SNSNotificationHandler? m_SNS;
		private string m_NotificationARN = "";

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			var jsonConfig = JObject.Parse(jsonFile);

			string accessKey  = jsonConfig["accessKey" ].ToObject<string>();
			string secretKey  = jsonConfig["secretKey" ].ToObject<string>();
			m_NotificationARN = jsonConfig["sns_arn"   ].ToObject<string>();
			string userTable  = jsonConfig["userTable" ].ToObject<string>();
			string guildTable = jsonConfig["guildTable"].ToObject<string>();

			var endpoint = RegionEndpoint.GetBySystemName(jsonConfig["endpoint"].ToObject<string>());

			var awsConfig = new AWSConfigService(accessKey, secretKey, endpoint);
			services.AddSingleton(awsConfig);

			m_DynamoDBClient = new AmazonDynamoDBClient(awsConfig.Credentials, awsConfig.Region);

			services.AddSingleton<UserConfigService>(new DynamoDBUserConfigService(m_DynamoDBClient, userTable));
			services.AddSingleton<GuildConfigService>((isp) => new DynamoDBGuildConfigService(isp.GetRequiredService<ConfigService>(), m_DynamoDBClient, guildTable));
			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
#if !DEBUG
			m_SNS = new SNSNotificationHandler(services.GetService<NotificationService>(), services.GetService<AWSConfigService>(), m_NotificationARN);
#endif
			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_DynamoDBClient.Dispose();
			m_SNS?.Dispose();
		}
	}
}
