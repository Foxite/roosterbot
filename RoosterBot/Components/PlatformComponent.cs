using System;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="Component"/> that integrates RoosterBot with a user interface, usually an instant messaging platform.
	/// </summary>
	public abstract class PlatformComponent : Component {
		/// <summary>
		/// The name of the platform that this component provides an interface for. When serializing data specific to any platform, use this property instead of <see cref="Component.Name"/>.
		/// </summary>
		/// <remarks>
		/// There may exist multiple independent components that provide support for the same platform. By design, platform-agnostic components should never notice which PlatformComponent
		/// is being used.
		/// 
		/// For example: There are multiple .NET libraries for Discord (such as Discord.NET, Disqord or DSharpPlus) and consequently there may be a PlatformComponent for each of these.
		/// It should be possible, at any time, to shut down the bot, remove the PlatformComponent that uses Discord.NET, and install another using Disqord, without any apparent
		/// consequences. For this reason, serialized <see cref="SnowflakeReference"/>s use the PlatformName instead of the component's name.
		/// </remarks>
		public abstract string PlatformName { get; }

		/// <summary>
		/// Start the connection to the platform.
		/// 
		/// </summary>
		protected abstract void Connect(IServiceProvider services);

		/// <summary>
		/// Stop the connection to the platform.
		/// </summary>
		protected abstract void Disconnect();

		internal void ConnectInternal(IServiceProvider services) => Connect(services);
		internal void DisconnectInternal() => Disconnect();

		public abstract object GetSnowflakeIdFromString(string input);
	}
}
