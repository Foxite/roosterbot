using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RoosterBot.Services {
	public class SNSService {
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private ConfigService m_ConfigService;

		public SNSService(ConfigService config) {
			m_SNSClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUWest3);
			m_ConfigService = config;
		}

		public async Task SendCriticalErrorNotificationAsync(string message) {
#if !DEBUG
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS (async)");
			await m_SNSClient.PublishAsync(new PublishRequest(m_ConfigService.SNSCriticalFailureARN, message));
#else
			await Task.CompletedTask; // Just await it instead of returning it to suppress the warning
#endif
		}
		
		public void SendCriticalErrorNotification(string message) {
#if !DEBUG
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS");
			m_SNSClient.Publish(new PublishRequest(m_ConfigService.SNSCriticalFailureARN, message));
#endif
		}
	}
}
