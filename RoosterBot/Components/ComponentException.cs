using System;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Thrown when there is a problem with a Component.
	/// </summary>
	[Serializable]
	public abstract class ComponentException : Exception {
		public ComponentException() { }
		public ComponentException(string message) : base(message) { }
		public ComponentException(string message, Exception inner) : base(message, inner) { }
		protected ComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown when a component cannot satisfy one of its requirements.
	/// </summary>
	[Serializable]
	public class ComponentDependencyException : ComponentException {
		public ComponentDependencyException() { }
		public ComponentDependencyException(string message) : base(message) { }
		public ComponentDependencyException(string message, Exception inner) : base(message, inner) { }
		protected ComponentDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception. Always has a message and an inner exception.
	/// </summary>
	[Serializable]
	public abstract class OuterComponentException : ComponentException {
		public Type CausingComponent { get; }

		public OuterComponentException(string message, Exception inner, Type causingComponent) : base(message, inner) {
			CausingComponent = causingComponent;
		}

		protected OuterComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during construction phase.
	/// </summary>
	[Serializable]
	public class ComponentConstructionException : OuterComponentException {
		public ComponentConstructionException(string message, Exception inner, Type causingComponent) : base(message, inner, causingComponent) { }
		protected ComponentConstructionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the services phase.
	/// </summary>
	[Serializable]
	public class ComponentServiceException : OuterComponentException {
		public ComponentServiceException(string message, Exception inner, Type causingComponent) : base(message, inner, causingComponent) { }
		protected ComponentServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the modules phase.
	/// </summary>
	[Serializable]
	public class ComponentModuleException : OuterComponentException {
		public ComponentModuleException(string message, Exception inner, Type causingComponent) : base(message, inner, causingComponent) { }
		protected ComponentModuleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
