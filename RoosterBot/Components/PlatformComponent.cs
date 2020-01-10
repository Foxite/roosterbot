using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="Component"/> that integrates RoosterBot with a user interface, usually an instant messaging platform.
	/// </summary>
	[JsonConverter(typeof(PlatformComponentConverter))]
	public abstract class PlatformComponent : Component {
		public abstract string PlatformName { get; }

		protected abstract Task ConnectAsync(IServiceProvider services);
		protected abstract Task DisconnectAsync();

		internal Task ConnectInternalAsync(IServiceProvider services) => ConnectAsync(services);
		internal Task DisconnectInternalAsync() => DisconnectAsync();

		public abstract object GetSnowflakeIdFromString(string input);
	}

	internal class PlatformComponentConverter : JsonConverter {
		public override bool CanConvert(Type objectType) => objectType.Equals(typeof(PlatformComponent));

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
			if (reader.TokenType == JsonToken.String && reader.Value != null) {
				return Program.Instance.Components.GetPlatform((string) reader.Value);
			} else {
				throw new FormatException("Something other than a non-null string was encountered.");
			}
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
			if (value is PlatformComponent platform) {
				writer.WriteToken(JsonToken.String, platform.PlatformName);
			} else {
				throw new NotSupportedException("This converter can only convert " + nameof(PlatformComponent));
			}
		}
	}
}
