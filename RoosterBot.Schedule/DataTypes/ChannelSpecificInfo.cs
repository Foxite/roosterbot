using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoosterBot.Schedule {
	// TODO should be moved to RoosterBot
	// I think I did this once but backed out of it, forgot why.
	// Go and find out.
	public abstract class ChannelSpecificInfo {
		private readonly IEnumerable<SnowflakeReference> m_AllowedChannels;

		protected ChannelSpecificInfo(IEnumerable<SnowflakeReference> allowedChannels) {
			m_AllowedChannels = allowedChannels;
		}

		/// <summary>
		/// When working with a <see cref="RoosterCommandContext"/>, you should always use <see cref="RoosterCommandContext.ChannelConfig"/>.ChannelReference
		/// and never <see cref="RoosterCommandContext.Channel"/>.
		/// </summary>
		public bool IsChannelAllowed(SnowflakeReference reference) => m_AllowedChannels.Contains(reference);
	}

	[Serializable]
	public class NoAllowedChannelsException : Exception {
		public NoAllowedChannelsException() { }
		public NoAllowedChannelsException(string message) : base(message) { }
		public NoAllowedChannelsException(string message, Exception inner) : base(message, inner) { }
		protected NoAllowedChannelsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
