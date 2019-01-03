using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RoosterBot.Services {
	// Note to self: The reason this class is not internal is because Discord.NET needs to inject this into EditableCmdModuleBase.
	public class SNSService : IDisposable {
#if !DEBUG
		private AmazonSimpleNotificationServiceClient m_SNSClient;
		private ConfigService m_ConfigService;
#endif

		internal SNSService(ConfigService config) {
#if !DEBUG
			m_SNSClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.EUWest3);
			
			m_ConfigService = config;
			Program.Instance.ProgramStopping += (o, e) => { Dispose(); };
#endif
		}

#if !DEBUG
		internal async Task SendCriticalErrorNotificationAsync(string message) {
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS (async)");
			try {
				await this.m_SNSClient.PublishAsync(m_ConfigService.SNSCriticalFailureARN, message);
			} catch (AmazonSimpleNotificationServiceException ex) {
				Logger.Log(Discord.LogSeverity.Error, "SNSService", "Failed to send error report to SNS (async)", ex);
			}
		}

		internal void SendCriticalErrorNotification(string message) {
			Logger.Log(Discord.LogSeverity.Info, "SNSService", "Sending error report to SNS (sync)");
			try {
				this.m_SNSClient.Publish(m_ConfigService.SNSCriticalFailureARN, message);
			} catch (AmazonSimpleNotificationServiceException ex) {
				Logger.Log(Discord.LogSeverity.Error, "SNSService", "Failed to send error report to SNS (sync)", ex);
			}
		}
#else
		internal Task SendCriticalErrorNotificationAsync(string message) {
			return Task.CompletedTask;
		}

		internal void SendCriticalErrorNotification(string message) { }
#endif

		#region IDisposable Support
		// Everything in this region was added by Visual Studio during code analysis, I don't understand most of it.
		// I mean, I do, but why would you have a second method for this? Why not just stick with a regular Dispose()? I don't see the reason.
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
#if !DEBUG
				if (disposing) {
					if (m_SNSClient != null) { // No idea why, but the code analyzer will complain if you use null coalescence for this.
						m_SNSClient.Dispose(); // It won't if you do it like this, and it won't even suggest you use coalescence.
					}
				}

				m_ConfigService = null;
#endif

				disposedValue = true;
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
