using System;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Thrown when a method attempts to retrieve a snowflake entity from a platform, but it does not exist.
	/// </summary>
	public class SnowflakeNotFoundException : Exception {
		public SnowflakeNotFoundException() { }
		public SnowflakeNotFoundException(string? message) : base(message) { }
		public SnowflakeNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
		protected SnowflakeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
