using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoosterBot {
	// TODO move AWS code (this and SNSService) to a new AWSComponent
	// Use AbstractNotificationService in RoosterBot
	internal class CloudWatchReporter : IDisposable {
		private AmazonCloudWatchClient m_Client;
		private DiscordSocketClient m_Discord;

		public CloudWatchReporter(DiscordSocketClient discord) {
			m_Client = new AmazonCloudWatchClient(RegionEndpoint.EUWest1);

			m_Discord = discord;
			m_Discord.Connected += ReportNormal;
			m_Discord.Disconnected += ReportDead;
		}

		private async Task ReportNormal() => await ReportState(CloudWatchState.Normal);
		private async Task ReportDead(Exception e) => await ReportState(CloudWatchState.Dead);

		private async Task ReportState(CloudWatchState state) {
#if !DEBUG
			await m_Client.PutMetricDataAsync(new PutMetricDataRequest() {
				Namespace = "RoosterBotReady",
				MetricData = new List<MetricDatum>() {
					new MetricDatum() {
						MetricName = "BotDead",
						TimestampUtc = DateTime.UtcNow,
						Unit = StandardUnit.None,
						Value = (int) state
					}
				}
			});
#endif
		}

		public void Dispose() {
			// Prevent Dead report because of shutdown
			m_Discord.Connected -= ReportNormal;
			m_Discord.Disconnected -= ReportDead;

			m_Client.Dispose();
			m_Client = null;
		}

		public enum CloudWatchState {
			Normal = 0,
			Dead = 1
		}
	}
}
