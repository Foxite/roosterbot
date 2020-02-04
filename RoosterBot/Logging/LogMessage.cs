using System;

namespace RoosterBot {
	/// <summary>
	/// A message for <see cref="Logger"/>.
	/// </summary>
	public struct LogMessage {
		/// <summary>
		/// The <see cref="LogLevel"/> of this message.
		/// </summary>
		public LogLevel Level { get; }

		/// <summary>
		/// The source of this message.
		/// </summary>
		public string Tag { get; }

		/// <summary>
		/// The actual message.
		/// </summary>
		public string Message { get; }
		
		/// <summary>
		/// The exception attached with this message, if any.
		/// </summary>
		public Exception? Exception { get; }

		/// <summary>
		/// Construct a new instance of LogMessage.
		/// </summary>
		/// <param name="level">The <see cref="LogLevel"/> of the message.</param>
		/// <param name="tag">The source of the message.</param>
		/// <param name="message">The actual message.</param>
		/// <param name="exception">The exception attached with the message, if any.</param>
		public LogMessage(LogLevel level, string tag, string message, Exception? exception = null) {
			Level = level;
			Tag = tag;
			Message = message;
			Exception = exception;
		}
	}
}
