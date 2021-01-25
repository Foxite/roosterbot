using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;

namespace RoosterBot.AWS {
	internal sealed class SNSNotificationHandler : IDisposable {
		private readonly AmazonSimpleNotificationServiceClient m_SNSClient;
		private readonly string m_ARN;

		public SNSNotificationHandler(NotificationService notificationService, AWSConfigService config, string arn) {
			m_SNSClient = new AmazonSimpleNotificationServiceClient(config.Credentials, new AmazonSimpleNotificationServiceConfig() {
				RegionEndpoint = config.Region
			});
			m_ARN = arn;

			notificationService.NotificationAdded += SendCriticalErrorNotificationAsync;
		}

		private async Task SendCriticalErrorNotificationAsync(NotificationEventArgs nea) {
			Logger.Info(AWSComponent.LogTag, "Sending error report to SNS");
			try {
				await m_SNSClient.PublishAsync(m_ARN, nea.Message);
			} catch (AmazonSimpleNotificationServiceException ex) {
				Logger.Error(AWSComponent.LogTag, "Failed to send error report to SNS", ex);
			}
		}

		public void Dispose() {
			if (m_SNSClient != null) {
				m_SNSClient.Dispose();
			}
		}
	}
}
