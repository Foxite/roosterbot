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

		public bool Equals(SnowflakeReference? other) {
			if (!(other is null)) {
				return Platform.PlatformName == other.Platform.PlatformName &&
					   // Here's the problem:
					   // When you serialize a ulong, it gets stored as a number.
					   // When you serialize an sbyte, it gets stored as a number.
					   // When you deserialize a number into a variable typed object, it will pick the smallest type that fits it.
					   // This means you can't serialize a ulong and expect to get back a ulong. That won't happen unless it's bigger than 2^63.
					   // When you compare a deserialized SnowflakeReference, to an SR obtained from a "live" snowflake, it may not work.
					   // As a solution, we compare the actual numeric value of the IDs (if it's a numeric type), which sidesteps the problem.
					   // But this is not ideal. An ideal solution allows us to store the type of the ID along with it, but it doesn't seem that Newtonsoft.Json
					   //  supports this, at least not for primitive types.
					   Util.CompareNumeric(Id, other.Id);
			} else {
				return false;
			}
		}

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
