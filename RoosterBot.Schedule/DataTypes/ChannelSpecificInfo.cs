using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoosterBot.Schedule {
	public abstract class ChannelSpecificInfo {
		private readonly IEnumerable<SnowflakeReference> m_AllowedChannels;

		protected ChannelSpecificInfo(IEnumerable<SnowflakeReference> allowedChannels) {
			m_AllowedChannels = allowedChannels;
		}

		public bool IsChannelAllowed(SnowflakeReference reference) => m_AllowedChannels.Contains(reference);
		public bool IsChannelAllowed(IChannel channel) => IsChannelAllowed(new SnowflakeReference(channel.Platform, channel.Id));
	}

	[Serializable]
	public class NoAllowedChannelsException : Exception {
		public NoAllowedChannelsException() { }
		public NoAllowedChannelsException(string message) : base(message) { }
		public NoAllowedChannelsException(string message, Exception inner) : base(message, inner) { }
		protected NoAllowedChannelsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
