namespace RoosterBot {
	/// <summary>
	/// Represents a unique entity on a given platform, identified by an ID.
	/// </summary>
	public interface ISnowflake {
		/// <summary>
		/// Gets the object used to uniquely identify this snowflake on the <see cref="Platform"/>.
		/// </summary>
		object Id { get; }

		/// <summary>
		/// Gets the platform that originated this snowflake.
		/// </summary>
		PlatformComponent Platform { get; }
	}
}
