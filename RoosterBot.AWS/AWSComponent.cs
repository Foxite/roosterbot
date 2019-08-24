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
		private SNSClient m_SNS;

		public override Task AddServices(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_NotificationARN = jsonConfig["sns"]["arn"].ToObject<string>();
			m_SNSEndpoint = jsonConfig["sns"]["endpoint"].ToObject<RegionEndpoint>();


			return Task.CompletedTask;
		}

		public override Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> _) {
			m_SNS = null;
#if !DEBUG
			m_SNS = new SNSClient(services.GetService<NotificationService>(), m_NotificationARN, m_SNSEndpoint);
#endif
			return Task.CompletedTask;
		}

		public override Task OnShutdown() {
			m_SNS?.Dispose();
			return Task.CompletedTask;
		}
	}
}
