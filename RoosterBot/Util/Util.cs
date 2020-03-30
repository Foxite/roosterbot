using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RoosterBot {
	/// <summary>
	/// A static class containing several helper functions.
	/// </summary>
	public static class Util {
		/// <summary>
		/// This will deserialize a Json file based on a template class. If the file does not exist, it will be created and the template will be serialized and written to the file.
		/// </summary>
		public static T LoadJsonConfigFromTemplate<T>(string filePath, T template, JsonSerializerSettings? jss = null) {
			if (File.Exists(filePath)) {
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), jss) ?? throw new InvalidOperationException("Configuration being deserialized is null.");
			} else {
				Logger.Warning("Main", "A config file was not found and a blank template has been created: " + filePath);
				Logger.Warning("Main", "You should read the file and change it where necessary. The template config will likely not function properly.");

				jss ??= new JsonSerializerSettings();
				jss.Converters.Add(new StringEnumConverter());
				jss.Formatting = Formatting.Indented;

				using var sw = File.CreateText(filePath);
				sw.Write(JsonConvert.SerializeObject(template, jss));
				return template;
			}
		}

		/// <summary>
		/// This returns true if <paramref name="input"/> is either:
		/// - Type of <typeparamref name="T"/>, in which case <paramref name="result"/> will be set to <paramref name="input"/>, or
		/// - A <see cref="CompoundResult"/> with only a single result which is type of <typeparamref name="T"/>, in which case <paramref name="result"/> will be set to that item.
		/// Otherwise this will return false and <paramref name="result"/> will be set to null.
		/// </summary>
		public static bool Is<T>(this RoosterCommandResult input, [MaybeNullWhen(false), NotNullWhen(true)] out T? result) where T : RoosterCommandResult {
			result =                                    // Hard-to-read expression - I've laid it out here:
				input as T ??                           // Simple, if result is T then return result as T.
				((input is CompoundResult cr            // If it's not T: Is it a compound result...
				&& cr.IndividualResults.CountEquals(1)) //  with only one item?
				? cr.IndividualResults.First() as T     //   Then return the first (and only) item as T, returning null if it's not T.
				: null);                                // otherwise return null.
			return result != null;
		}

		/// <summary>
		/// Get a <see cref="SnowflakeReference"/> for this <see cref="ISnowflake"/>.
		/// </summary>
		public static SnowflakeReference GetReference(this ISnowflake snowflake) => new SnowflakeReference(snowflake.Platform, snowflake.Id);

		/// <summary>
		/// - If <paramref name="key"/> exists in <paramref name="dict"/>, then the value stored at that key will be returned.
		/// - Otherwise, <paramref name="addValue"/> will be added to <paramref name="dict"/> using <paramref name="key"/>, and then returned from this function.
		/// </summary>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue addValue) where TKey : notnull {
			return dict.GetOrAdd(key, _ => addValue);
		}

		/// <summary>
		/// - If <paramref name="key"/> exists in <paramref name="dict"/>, then the value stored at that key will be returned.
		/// - Otherwise, the return value of <paramref name="addFactory"/> will be added to <paramref name="dict"/> using <paramref name="key"/>, and then returned from this function.
		/// </summary>
		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> addFactory) where TKey : notnull {
			if (dict.TryGetValue(key, out TValue value)) {
				return value;
			} else {
				TValue ret = addFactory(key);
				dict.Add(key, ret);
				return ret;
			}
		}

		/// <summary>
		/// Converts a non-generic <see cref="DictionaryEntry"/> to a generic <see cref="KeyValuePair{TKey, TValue}"/>.
		/// </summary>
		public static KeyValuePair<TKey, TValue> ToGeneric<TKey, TValue>(this DictionaryEntry de) {
			return new KeyValuePair<TKey, TValue>((TKey) de.Key, (TValue) de.Value!);
		}
	}
}
