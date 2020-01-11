using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace RoosterBot {
	/// <summary>
	/// Contains data needed to obtain an <see cref="ISnowflake"/> object.
	/// This class can be instantiated before a platform connection is available, allowing it to be deserialized from config data at startup.
	/// </summary>
	[JsonConverter(typeof(SnowflakeReferenceConverter))]
	[DebuggerDisplay("{Platform.PlatformName}:{Id.ToString()}")]
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
					   Id.Equals(other.Id);
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

	internal class SnowflakeReferenceConverter : JsonConverter<SnowflakeReference> {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Anonymous type cannot be used as generic type, must be inferred")]
		public override SnowflakeReference ReadJson(JsonReader reader, Type objectType, SnowflakeReference? existingValue, bool hasExistingValue, JsonSerializer serializer) {
			T deserializeAnonymous<T>(T template) {
				return serializer.Deserialize<T>(reader);
			}

			var rawResult = deserializeAnonymous(new {
				Platform = "",
				Id = ""
			});

			PlatformComponent? platform = Program.Instance.Components.GetPlatform(rawResult.Platform);
			if (platform == null) {
				throw new KeyNotFoundException($"Platform named {rawResult.Platform} is not installed. Cannot deserialize a SnowflakeReference for this platform.");
			}
			return new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(rawResult.Id));
		}

		public override void WriteJson(JsonWriter writer, SnowflakeReference? value, JsonSerializer serializer) {
			if (value == null) {
				throw new InvalidOperationException("Cannot deserialize a null SnowflakeReference");
			}

			serializer.Serialize(writer, new {
				Platform = value.Platform.PlatformName,
				Id = value.Id.ToString()
			});
		}
	}
}
