using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoosterBot.Schedule {
	public abstract class ChannelSpecificInfo {
		private readonly IEnumerable<object> m_AllowedChannels;

		protected ChannelSpecificInfo(IEnumerable<object> allowedChannels) {
			m_AllowedChannels = allowedChannels;
		}

		public bool IsGuildAllowed(object channelId) => m_AllowedChannels.Contains(channelId);
		public bool IsGuildAllowed(IChannel channel) => IsGuildAllowed(channel.Id);
	}


	[Serializable]
	public class NoAllowedChannelsException : Exception {
		public NoAllowedChannelsException() { }
		public NoAllowedChannelsException(string message) : base(message) { }
		public NoAllowedChannelsException(string message, Exception inner) : base(message, inner) { }
		protected NoAllowedChannelsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
