using System;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Exception thrown when an error has been caused by a component.
	/// </summary>
	[Serializable]
	internal class ComponentException : Exception {
		public ComponentException() { }
		public ComponentException(string message) : base(message) { }
		public ComponentException(string message, Exception innerException) : base(message, innerException) { }
		protected ComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}