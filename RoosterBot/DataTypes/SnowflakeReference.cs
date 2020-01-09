using System;

namespace RoosterBot {
	/// <summary>
	/// Contains data needed to obtain an <see cref="ISnowflake"/> object.
	/// This class can be instantiated before a platform connection is available, allowing it to be deserialized from config data at startup.
	/// </summary>
	public class SnowflakeReference : IEquatable<SnowflakeReference> {
		public PlatformComponent Platform { get; }
		public object Id { get; }

		public SnowflakeReference(PlatformComponent platform, object id) {
			Platform = platform;
			Id = id;
		}

		public override bool Equals(object? obj) => Equals(obj as SnowflakeReference);

		public bool Equals(SnowflakeReference? other) =>
			!(other is null)
			&& Platform.PlatformName == other.Platform.PlatformName
			&& Id.Equals(other.Id);

		public override int GetHashCode() => HashCode.Combine(Platform, Id);

		public static bool operator ==(SnowflakeReference? left, SnowflakeReference? right) {
			if (left is null && right is null) {
				return true;
			}

			if ((left is null) != (right is null)) {
				return false;
			}

			return left!.Equals(right);
		}

		public static bool operator !=(SnowflakeReference? left, SnowflakeReference? right) {
			return !(left == right);
		}
	}
}
