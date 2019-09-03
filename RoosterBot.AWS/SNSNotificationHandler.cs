using Amazon.SimpleNotificationService;
using System;
using System.Threading.Tasks;

namespace RoosterBot.AWS {
	public class SNSNotificationHandler : IDisposable {
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private string m_ARN;

		public SNSNotificationHandler(NotificationService notificationService, AWSConfigService config, string arn) {
			m_SNSClient = new AmazonSimpleNotificationServiceClient(config.Credentials, new AmazonSimpleNotificationServiceConfig() {
				RegionEndpoint = config.Region,
				EndpointDiscoveryEnabled = true
			});
			m_ARN = arn;

			notificationService.NotificationAdded += SendCriticalErrorNotificationAsync;
		}

		private async Task SendCriticalErrorNotificationAsync(string message) {
			Logger.Info("SNSService", "Sending error report to SNS");
			try {
				await m_SNSClient.PublishAsync(m_ARN, message);
			} catch (AmazonSimpleNotificationServiceException ex) {
				Logger.Error("SNSService", "Failed to send error report to SNS", ex);
			}
		}

		#region IDisposable Support
		private bool m_Disposed = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!m_Disposed) {
				if (disposing) {
					if (m_SNSClient != null) {
						m_SNSClient.Dispose();
					}
				}
				m_Disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}
