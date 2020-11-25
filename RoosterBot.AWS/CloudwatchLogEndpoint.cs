using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;

namespace RoosterBot.AWS {
	internal sealed class CloudwatchLogEndpoint : LogEndpoint, IDisposable {
		private readonly BufferBlock<InputLogEvent> m_MessageQueue;
		private readonly PutLogEventsRequest m_RequestTemplate;
		private readonly AmazonCloudWatchLogsClient m_Client;
		private readonly Timer m_ProcessTimer;

		private CloudwatchLogEndpoint(AmazonCloudWatchLogsClient client, string groupName, string streamName, TimeSpan processPeriod) {
			m_RequestTemplate = new PutLogEventsRequest() {
				LogGroupName = groupName,
				LogStreamName = streamName
			};

			m_Client = client;
			m_MessageQueue = new BufferBlock<InputLogEvent>(new DataflowBlockOptions() {
				EnsureOrdered = true
			});

			m_ProcessTimer = new Timer(ProcessQueue, null, (uint) processPeriod.TotalMilliseconds, (uint) processPeriod.TotalMilliseconds);
		}

		internal static async Task<CloudwatchLogEndpoint> CreateAsync(AmazonCloudWatchLogsClient logClient, TimeSpan processPeriod) {
			string logStreamName = "Instance/" + Process.GetCurrentProcess().StartTime.ToUniversalTime().ToString("u").Replace(':', '-');

			LogGroup existingGroup = (await logClient.DescribeLogGroupsAsync(new DescribeLogGroupsRequest() {
				LogGroupNamePrefix = "RoosterBot"
			})).LogGroups.First(); // TODO chicken egg problem

			await logClient.CreateLogStreamAsync(new CreateLogStreamRequest(existingGroup.LogGroupName, logStreamName));

			return new CloudwatchLogEndpoint(logClient, existingGroup.LogGroupName, logStreamName, processPeriod);
		}

		public override void Log(LogMessage message) {
			string logString = $"[{message.Level}] {message.Tag} : {message.Message}";

			if (message.Exception != null) {
				logString += "\n" + message.Exception.ToStringDemystified();
			}

			m_MessageQueue.Post(new InputLogEvent() {
				Timestamp = DateTime.UtcNow,
				Message = logString
			});
		}

		private void ProcessQueue(object? state) {
			if (m_MessageQueue.TryReceiveAll(out var messages)) {
				// The api takes a List object despite it being convention to take an interface (such as IList). This is the reason.
				// In practice you don't care what specific type of object your parameters are, only what you can do with them. Therefore it doesn't MATTER if you get an array
				//  or a list or some other crazy type, as long as you can tell how many there are and enumerate them.
				// If this property was an ICollection there would be no problem and I wouldn't have to call .ToList() on the IList.
				m_RequestTemplate.LogEvents = messages.ToList();

				var putResult = m_Client.PutLogEventsAsync(m_RequestTemplate).Result;

				m_RequestTemplate.SequenceToken = putResult.NextSequenceToken;
			}
		}

		public void Dispose() {
			m_ProcessTimer.Dispose();
		}
	}
}