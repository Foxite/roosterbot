using System;
using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="Component"/> that integrates RoosterBot with a user interface, usually an instant messaging platform.
	/// </summary>
	public abstract class PlatformComponent : Component {
		public abstract string PlatformName { get; }

		protected abstract Task ConnectAsync(IServiceProvider services);
		protected abstract Task DisconnectAsync();

		internal Task ConnectInternalAsync(IServiceProvider services) => ConnectAsync(services);
		internal Task DisconnectInternalAsync() => DisconnectAsync();

		public abstract object GetSnowflakeIdFromString(string input);
	}
}
