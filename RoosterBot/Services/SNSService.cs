using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RoosterBot.Services {
	public class SNSService {
#pragma warning disable CS0169
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private ConfigService m_ConfigService;
#pragma warning restore CS0169

		internal SNSService(ConfigService config) {
#if !DEBUG
			m_SNSClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUWest3);
			m_ConfigService = config;
#endif
		}

		internal async Task SendCriticalErrorNotificationAsync(string message) {
#if !DEBUG
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS (async)");
			await m_SNSClient.PublishAsync(new PublishRequest(m_ConfigService.SNSCriticalFailureARN, message));
#else
			await Task.CompletedTask; // Just await it instead of returning it to suppress the warning
#endif
		}
		
		internal void SendCriticalErrorNotification(string message) {
#if !DEBUG
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS");
			m_SNSClient.Publish(new PublishRequest(m_ConfigService.SNSCriticalFailureARN, message));
#endif
		}
	}
}
