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
			await m_SNSClient.PublishAsync(new PublishRequest(m_ConfigService.SNSCriticalFailureARN, message));
		}
#else
		internal Task SendCriticalErrorNotificationAsync(string message) {
			return Task.CompletedTask;
		}
#endif
		
		#region IDisposable Support
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
