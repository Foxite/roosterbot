using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// An abstract type representing information that is restricted to a collection of <see cref="IChannel"/>s.
	/// </summary>
	public abstract class ChannelSpecificInfo {
		/// <summary>
		/// The collection of <see cref="IChannel"/> references that this object is restricted to.
		/// </summary>
		public IReadOnlyCollection<SnowflakeReference> AllowedChannels { get; }

		///
		protected ChannelSpecificInfo(IReadOnlyCollection<SnowflakeReference> allowedChannels) {
			AllowedChannels = allowedChannels;
		}

		/// <summary>
		/// When working with a <see cref="RoosterCommandContext"/>, you should always use <see cref="RoosterCommandContext.ChannelConfig"/>.ChannelReference
		/// and never <see cref="RoosterCommandContext.Channel"/>.
		/// </summary>
		public bool IsChannelAllowed(SnowflakeReference reference) => AllowedChannels.Contains(reference);
	}

	/// <summary>
	/// Thrown in situations where there is no <see cref="ChannelSpecificInfo"/> that is allowed to be used.
	/// </summary>
	[Serializable]
	public class NoAllowedChannelsException : Exception {
		/// <inheritdoc />
		public NoAllowedChannelsException() { }

		/// <inheritdoc />
		public NoAllowedChannelsException(string message) : base(message) { }

		/// <inheritdoc />
		public NoAllowedChannelsException(string message, Exception inner) : base(message, inner) { }

		/// <inheritdoc />
		protected NoAllowedChannelsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
