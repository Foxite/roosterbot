using System;
using System.Runtime.Serialization;

namespace RoosterBot.Watson {
	/// <summary>
	/// Thrown by WatsonClient when an exception occurs during parsing.
	/// </summary>
	[Serializable]
	public class WatsonException : Exception {
		public WatsonException() { }
		public WatsonException(string message) : base(message) { }
		public WatsonException(string message, Exception innerException) : base(message, innerException) { }
		protected WatsonException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}