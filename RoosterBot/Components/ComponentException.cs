using System;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Thrown when there is a problem with a Component.
	/// </summary>
	[Serializable]
	public abstract class ComponentException : Exception {
		private const string CausingComponentSerializedName = "CausingComponent"; // Never ever change this. Otherwise, exisiting serialized instances of this class will break.

		public Type CausingComponent { get; }

		public ComponentException(string message, Type causingComponent, Exception inner = null) : base(message, inner) {
			CausingComponent = causingComponent;
		}

		protected ComponentException(SerializationInfo info, StreamingContext context) : base(info, context) {
			CausingComponent = (Type) info.GetValue(CausingComponentSerializedName, typeof(Type));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue(CausingComponentSerializedName, CausingComponent); // Idk but Type is not serializable, I'm not sure if this is supposed to work. /shrug
		}
	}

	/// <summary>
	/// Thrown when a component cannot satisfy one of its requirements.
	/// </summary>
	[Serializable]
	public class ComponentDependencyException : ComponentException {
		public ComponentDependencyException(string message, Type causingComponent) : base(message, causingComponent) { }
		protected ComponentDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception. Always has an inner exception.
	/// </summary>
	[Serializable]
	public abstract class OuterComponentException : ComponentException {
		public OuterComponentException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		protected OuterComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during construction phase.
	/// </summary>
	[Serializable]
	public class ComponentConstructionException : OuterComponentException {
		public ComponentConstructionException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		protected ComponentConstructionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the services phase.
	/// </summary>
	[Serializable]
	public class ComponentServiceException : OuterComponentException {
		public ComponentServiceException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		protected ComponentServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the install phase.
	/// </summary>
	[Serializable]
	public class ComponentInstallationException : OuterComponentException {
		public ComponentInstallationException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		protected ComponentInstallationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the modules phase.
	/// </summary>
	[Serializable]
	public class ComponentModuleException : OuterComponentException {
		public ComponentModuleException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		protected ComponentModuleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown if one or more services were not successfully installed.
	/// </summary>
	[Serializable]
	public class ServiceInstallationException : Exception {
		public ServiceInstallationException() { }
		public ServiceInstallationException(string message) : base(message) { }
		public ServiceInstallationException(string message, Exception innerException) : base(message, innerException) { }
		protected ServiceInstallationException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
	}
}
