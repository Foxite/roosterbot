﻿using Amazon;
using Amazon.SimpleNotificationService;
using System;
using System.Threading.Tasks;

namespace RoosterBot.AWS {
	public class SNSClient : IDisposable {
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private string m_ARN;

		public SNSClient(NotificationService notificationService, string arn, RegionEndpoint endpoint) {
			m_SNSClient = new AmazonSimpleNotificationServiceClient(endpoint);
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
					m_SNSClient?.Dispose();
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
