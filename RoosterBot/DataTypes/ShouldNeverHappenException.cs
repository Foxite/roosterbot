using System;

namespace RoosterBot {
	/// <summary>
	/// Thrown in situations that should not have occurred.
	/// </summary>
	internal class ShouldNeverHappenException : Exception {
		public ShouldNeverHappenException() { }
		public ShouldNeverHappenException(string message) : base(message) { }
		public ShouldNeverHappenException(string message, Exception innerException) : base(message, innerException) { }
	}
}
