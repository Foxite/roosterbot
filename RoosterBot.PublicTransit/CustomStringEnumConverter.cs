using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace RoosterBot.PublicTransit {
	/// <summary>
	/// An alternative to the builtin StringEnumConverter that lets you use <see cref="JsonPropertyAttribute"/> to change the name used to (de)serialize the enum member.
	/// </summary>
	public class CustomStringEnumConverter : JsonConverter {
		private static readonly Dictionary<Type, Dictionary<string, object>> EnumCache = new Dictionary<Type, Dictionary<string, object>>();

		private static Dictionary<string, object> GetCache(Type objectType) {
			if (!EnumCache.TryGetValue(objectType, out Dictionary<string, object>? cache)) {
				cache = EnumCache[objectType] = objectType
					.GetFields(BindingFlags.Public | BindingFlags.Static)
					.ToDictionary(
						field => field.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? field.Name,
						field => field.GetValue(null)! // Should never actually return null
					);
			}

			return cache;
		}

		public override bool CanConvert(Type objectType) => objectType.IsEnum;
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
			Dictionary<string, object> dictionaries = GetCache(objectType);
			return reader.Value is string key ? dictionaries[key] : existingValue;
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
			if (value is not null) {
				Dictionary<string, object> dictionaries = GetCache(value.GetType());
				writer.WriteValue(dictionaries.First(kvp => {
					return kvp.Value.Equals(value);
				}).Key);
			}
		}
	}
}
