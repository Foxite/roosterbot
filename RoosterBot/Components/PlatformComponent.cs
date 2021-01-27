using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Returns the type used for the Id of all <see cref="ISnowflake"/> produced by this platform.
		/// </summary>
		public abstract Type SnowflakeIdType { get; }

		/// <summary>
		/// Start the connection to the platform.
		/// </summary>
		protected abstract void Connect(IServiceProvider services);

		/// <summary>
		/// Stop the connection to the platform.
		/// </summary>
		protected abstract void Disconnect();

		internal void ConnectInternal(IServiceProvider services) => Connect(services);
		internal void DisconnectInternal() => Disconnect();

		/// <summary>
		/// Get a platform-specific object that can be used as an <see cref="ISnowflake.Id"/>.
		/// </summary>
		public abstract object GetSnowflakeIdFromString(string input);

		private readonly List<ResultAdapter> m_ResultAdapters = new();
		
		/// <summary>
		/// Register a <see cref="ResultAdapter"/> for this <see cref="PlatformComponent"/> to use.
		/// </summary>
		public void RegisterResultAdapter(ResultAdapter adapter) {
			m_ResultAdapters.Add(adapter);
		}

		/// <summary>
		/// Enumerate registered <see cref="ResultAdapter"/>s that can process the given <paramref name="result"/> for the given <paramref name="context"/>.
		/// </summary>
		public IEnumerable<ResultAdapter> GetResultAdapter(RoosterCommandContext context, RoosterCommandResult result) {
			return m_ResultAdapters.Where(adapter => adapter.CanHandleResult(context, result));
		}
	}
}
