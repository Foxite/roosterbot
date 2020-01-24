namespace RoosterBot {
	/// <summary>
	/// A static class containing some helper functions for dealing with platforms.
	/// </summary>
	public static class PlatformUtil {
		/// <summary>
		/// Get a <see cref="SnowflakeReference"/> for this <see cref="ISnowflake"/>.
		/// </summary>
		public static SnowflakeReference GetReference(this ISnowflake snowflake) => new SnowflakeReference(snowflake.Platform, snowflake.Id);
	}
}
