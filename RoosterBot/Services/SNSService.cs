using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;

namespace RoosterBot.Services {
	internal class SNSService : AbstractSNSService, IDisposable {
#if DEBUG
		public SNSService(ConfigService config) : base(config) { }

		public void Dispose() { }
#else
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private ConfigService m_ConfigService;

		public SNSService(ConfigService config) : base(config) {
			m_SNSClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUWest3);

			m_ConfigService = config;
		}

		public async override Task SendCriticalErrorNotificationAsync(string message) {
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS (async)");
			try {
				await m_SNSClient.PublishAsync(m_ConfigService.SNSCriticalFailureARN, message);
			} catch (AmazonSimpleNotificationServiceException ex) {
				Logger.Log(Discord.LogSeverity.Error, "SNSService", "Failed to send error report to SNS (async)", ex);
			}
			await Task.CompletedTask;
		}

		#region IDisposable Support
		// Everything in this region was added by Visual Studio during code analysis, I don't understand most of it.
		// I mean, I do, but why would you have a second method for this? Why not just stick with a regular Dispose()? I don't see the reason.
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (m_SNSClient != null) { // No idea why, but the code analyzer will complain if you use null coalescence for this.
						m_SNSClient.Dispose(); // It won't if you do it like this, and it won't even suggest you use coalescence.
					}
				}
				m_ConfigService = null;

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
#endif
	}

	internal abstract class AbstractSNSService {
#pragma warning disable IDE0060 // Remove unused parameter
		public AbstractSNSService(ConfigService config) { }
#pragma warning restore IDE0060 // Remove unused parameter

		public virtual Task SendCriticalErrorNotificationAsync(string message) => Task.CompletedTask;
	}
}
