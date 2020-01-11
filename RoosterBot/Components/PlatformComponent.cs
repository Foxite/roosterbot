using System;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="Component"/> that integrates RoosterBot with a user interface, usually an instant messaging platform.
	/// </summary>
	public abstract class PlatformComponent : Component {
		public abstract string PlatformName { get; }

		protected abstract void Connect(IServiceProvider services);
		protected abstract void Disconnect();

		internal void ConnectInternal(IServiceProvider services) => Connect(services);
		internal void DisconnectInternal() => Disconnect();

		public abstract object GetSnowflakeIdFromString(string input);
	}
}
