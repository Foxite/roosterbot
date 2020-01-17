using System;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Thrown when there is a problem with a Component.
	/// </summary>
	[Serializable]
	public abstract class ComponentException : Exception {
		private const string CausingComponentSerializedName = "CausingComponent"; // Never ever change this. Otherwise, exisiting serialized instances of this class will break.

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="Component"/> that caused this exception.
		/// </summary>
		public Type CausingComponent { get; }

		/// <inheritdoc/>
		protected ComponentException(string message, Type causingComponent, Exception? inner = null) : base(message, inner) {
			CausingComponent = causingComponent;
		}

		/// <inheritdoc/>
		protected ComponentException(SerializationInfo info, StreamingContext context) : base(info, context) {
			CausingComponent = (Type) (info.GetValue(CausingComponentSerializedName, typeof(Type)) ?? throw new SerializationException("The serialized value of CausingComponent is null"));
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue(CausingComponentSerializedName, CausingComponent); // Idk but Type is not serializable, I'm not sure if this is supposed to work. /shrug
		}
	}

	/// <summary>
	/// Thrown when a component cannot satisfy one of its requirements.
	/// </summary>
	[Serializable]
	public class ComponentDependencyException : ComponentException {
		/// <inheritdoc/>
		public ComponentDependencyException(string message, Type causingComponent) : base(message, causingComponent) { }
		/// <inheritdoc/>
		protected ComponentDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception. Always has an inner exception.
	/// </summary>
	[Serializable]
	public abstract class OuterComponentException : ComponentException {
		/// <inheritdoc/>
		protected OuterComponentException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		/// <inheritdoc/>
		protected OuterComponentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during construction phase.
	/// </summary>
	[Serializable]
	public class ComponentConstructionException : OuterComponentException {
		/// <inheritdoc/>
		public ComponentConstructionException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		/// <inheritdoc/>
		protected ComponentConstructionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the services phase.
	/// </summary>
	[Serializable]
	public class ComponentServiceException : OuterComponentException {
		/// <inheritdoc/>
		public ComponentServiceException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		/// <inheritdoc/>
		protected ComponentServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception during the modules phase.
	/// </summary>
	[Serializable]
	public class ComponentModuleException : OuterComponentException {
		/// <inheritdoc/>
		public ComponentModuleException(string message, Type causingComponent, Exception inner) : base(message, causingComponent, inner) { }
		/// <inheritdoc/>
		protected ComponentModuleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
