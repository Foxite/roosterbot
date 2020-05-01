using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	/// <summary>
	/// A static class containing several helper functions.
	/// </summary>
	public static class Util {
		/// <summary>
		/// This will deserialize a Json file based on a template class. If the file does not exist, it will be created and the template will be serialized and written to the file.
		/// </summary>
		public static T LoadJsonConfigFromTemplate<T>(string filePath, T template, JsonSerializerSettings? jss = null) where T : notnull {
			jss ??= new JsonSerializerSettings();
			jss.Converters.Add(new StringEnumConverter());
			jss.Formatting = Formatting.Indented;

			if (File.Exists(filePath)) {
				string key = "null";
				try {
					// JsonConvert.PopulateObject exists, but since anonymous types only have readonly properties, you can't populate those objects.
					// But they have a constructor which JsonConvert.Deserialize uses.
					// The only problem is that it doesn't get default values from anywhere, so it passes null into missing parameters.
					// But we HAVE default values, only Newtonsoft.Json does not provide a way to pass them into Deserialize.
					// I implemented my own logic for this and gave it good error reporting while I was at it.
					var messages = new List<string>();
					bool writeBack = false;

					object populateObject(JObject source, object template) {
						var parameters = new Dictionary<string, object?>();

						foreach (var kvp in source) {
							key = kvp.Key;
							PropertyInfo? prop = template.GetType().GetProperty(kvp.Key);

							if (prop == null) {
								messages.Add($"Contains unrecognized key {kvp.Key}");
							} else {
								if (prop.SetMethod == null || !prop.SetMethod.IsPublic) {
									messages.Add($"Property {prop.Name} does not have a public setter and cannot be used");
									continue;
								}

								if (kvp.Value == null) {
									parameters[kvp.Key] = null;
								} else if (kvp.Value.Type != JTokenType.Object) {
									parameters[kvp.Key] = kvp.Value.ToObject(prop.PropertyType);
								} else {
									// TODO distinguish between dictionaries and other types
									// Currently only does types, fucks out when you do dicts
									parameters[kvp.Key] = populateObject((JObject) kvp.Value, prop.GetValue(template) ?? Activator.CreateInstance(prop.PropertyType)!);
								}
							}
						}

						// TODO activate class with parameters

						// Template keys missing from config file
						IEnumerable<MemberInfo> missingKeys = obj.GetType().GetMembers()
								.Where(member => member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
								//.Select(member => member.Name)
								.Where(member => source.ContainsKey(member.Name));

						foreach (MemberInfo member in missingKeys) {
							source[member.Name] = JToken.FromObject(((dynamic) member).GetValue(obj));
							messages.Add("Missing key " + member.Name + " (it has been added and set to the default value)");
							writeBack = true;
						}

						if (messages.Any()) {
							Logger.Warning("Main", "A config file was found but had one or more problems: " + filePath + "\n" + string.Join("\n- ", messages));
						}
					}

					var source = JObject.Parse(File.ReadAllText(filePath));
					populateObject(source, template);

					if (writeBack) {
						File.WriteAllText(filePath, JsonConvert.SerializeObject(source, jss));
					}

					return template;
				} catch {
					Logger.Critical("Main", "Crash occured at " + key);
					throw;
				}
			} else {
				Logger.Warning("Main", "A config file was not found and a blank template has been created: " + filePath + "\n" +
					"You should read the file and change it where necessary. The template config will likely not function properly.");

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
