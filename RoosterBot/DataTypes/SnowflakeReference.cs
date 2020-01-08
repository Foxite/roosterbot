namespace RoosterBot {
	/// <summary>
	/// Contains data needed to obtain an <see cref="ISnowflake"/> object.
	/// </summary>
	public class SnowflakeReference {
		public PlatformComponent Platform { get; }
		public object Id { get; }

		public SnowflakeReference(PlatformComponent platform, object id) {
			Platform = platform;
			Id = id;
		}
	}
}
