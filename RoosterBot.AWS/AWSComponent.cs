using System;
using System.IO;
using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.AWS {
	public class AWSComponent : Component {
		public override Version ComponentVersion => new Version(1, 2, 1);

#nullable disable
		private AmazonDynamoDBClient m_DynamoDBClient;
#nullable restore
		// This field may actually be null after startup, because it does not get created in debug builds. So don't exclude it from nullability.
		private SNSNotificationHandler? m_SNS;
		private string m_NotificationARN = "";

		protected override void AddServices(IServiceCollection services, string configPath) {
			var jsonConfig = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				AccessKey = "",
				SecretKey = "",
				NotificationArn = "",
				UserTable = "",
				ChannelTable = "",
				Endpoint  = "",
			});

			m_NotificationARN = jsonConfig.NotificationArn;

			var awsConfig = new AWSConfigService(jsonConfig.AccessKey, jsonConfig.SecretKey, RegionEndpoint.GetBySystemName(jsonConfig.Endpoint));
			services.AddSingleton(awsConfig);

			m_DynamoDBClient = new AmazonDynamoDBClient(awsConfig.Credentials, awsConfig.Region);

			services.AddSingleton<UserConfigService>(new DynamoDBUserConfigService(m_DynamoDBClient, jsonConfig.UserTable));
			services.AddSingleton<ChannelConfigService>((isp) => new DynamoDBChannelConfigService(isp.GetRequiredService<GlobalConfigService>(), m_DynamoDBClient, jsonConfig.ChannelTable));
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			bool production = true;
			// This way, there will be no warnings about unused fields.
#if DEBUG
			production = false;
#endif
			if (production) {
				m_SNS = new SNSNotificationHandler(services.GetService<NotificationService>(), services.GetService<AWSConfigService>(), m_NotificationARN);
			}
		}

		protected override void Dispose(bool disposing) {
			m_DynamoDBClient.Dispose();
			m_SNS?.Dispose();
		}
	}
}
