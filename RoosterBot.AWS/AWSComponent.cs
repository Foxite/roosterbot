#if DEBUG
#pragma warning disable IDE0052 // Private member assigned but never used
#pragma warning disable CS0649 // Field never assigned to
#endif

using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.AWS {
	public class AWSComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		private string m_NotificationARN;
		private AWSConfigService m_AWSConfig;
		private SNSNotificationHandler m_SNS;

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_NotificationARN = jsonConfig["sns_arn"].ToObject<string>();

			string accessKey = jsonConfig["accessKey"].ToObject<string>();
			string secretKey = jsonConfig["secretKey"].ToObject<string>();
			RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(jsonConfig["endpoint"].ToObject<string>());

			m_AWSConfig = new AWSConfigService(accessKey, secretKey, endpoint);
			services.AddSingleton(m_AWSConfig);

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> _) {
#if !DEBUG
			m_SNS = new SNSNotificationHandler(services.GetService<NotificationService>(), m_AWSConfig, m_NotificationARN);
#endif
			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_SNS?.Dispose();
		}
	}
}
