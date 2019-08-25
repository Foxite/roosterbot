#if DEBUG
#pragma warning disable IDE0052 // Private member assigned but never used
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
		private RegionEndpoint m_SNSEndpoint;
		private SNSNotificationHandler m_SNS;

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_NotificationARN = jsonConfig["sns"]["arn"].ToObject<string>();
			m_SNSEndpoint = RegionEndpoint.GetBySystemName(jsonConfig["sns"]["endpoint"].ToObject<string>());

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> _) {
#if !DEBUG
			m_SNS = new SNSNotificationHandler(services.GetService<NotificationService>(), m_NotificationARN, m_SNSEndpoint);
#endif
			return Task.CompletedTask;
		}

		public override Task ShutdownAsync() {
			m_SNS?.Dispose();
			return Task.CompletedTask;
		}
	}
}
