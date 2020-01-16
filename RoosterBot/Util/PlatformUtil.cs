using System;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// A static class containing some helper functions for dealing with platforms.
	/// </summary>
	public static class PlatformUtil {
		/// <summary>
		/// This allows any component to convert strings (such as stored in config files) into objects that can be compared to <see cref="ISnowflake.Id"/> values,
		/// without having to directly interact with any PlatformComponent.
		/// </summary>
		public static PlatformComponent? GetPlatformComponent(string platformName) {
			return Program.Instance.Components.GetComponents().OfType<PlatformComponent>().Where(pc => pc.PlatformName == platformName).SingleOrDefault();
		}

		/// <summary>
		/// Construct a <see cref="SnowflakeReference"/> using the installed <see cref="PlatformComponent"/> for <paramref name="platformName"/>.
		/// </summary>
		public static SnowflakeReference GetSnowflakeReference(string platformName, string snowflakeString) {
			PlatformComponent? platform = GetPlatformComponent(platformName);
			if (platform == null) {
				throw new InvalidOperationException("Cannot find a PlatformComponent for " + platformName);
			}
			return new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(snowflakeString));
		}

		/// <summary>
		/// Get a <see cref="SnowflakeReference"/> for this <see cref="ISnowflake"/>.
		/// </summary>
		public static SnowflakeReference GetReference(this ISnowflake snowflake) => new SnowflakeReference(snowflake.Platform, snowflake.Id);
	}
}
