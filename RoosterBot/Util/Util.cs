using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RoosterBot {
	public static class Util {
		private static bool s_FileCreationWarningNotSent = true;

		public static readonly Random RNG = new Random();

		/// <summary>
		/// This will deserialize a Json file based on a template class. If the file does not exist, it will be created and the template will be serialized and written to the file.
		/// </summary>
		public static T LoadJsonConfigFromTemplate<T>(string filePath, T template, JsonSerializerSettings? jss = null) {
			if (File.Exists(filePath)) {
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), jss);
			} else {
				if (s_FileCreationWarningNotSent) {
					s_FileCreationWarningNotSent = false;
					Logger.Warning("Main", "A config file was not found and a blank template has been created: " + filePath);
					Logger.Warning("Main", "You should read the file and change it where necessary. The template config will likely not function properly.");
				}

				jss ??= new JsonSerializerSettings();
				jss.Converters.Add(new StringEnumConverter());
				jss.Formatting = Formatting.Indented;

				using var sw = File.CreateText(filePath);
				sw.Write(JsonConvert.SerializeObject(template, jss));
				return template;
			}
		}

		public static bool Is<T>(this RoosterCommandResult input, [MaybeNullWhen(false), NotNullWhen(true)] out T? result) where T : RoosterCommandResult {
			result =                                    // Hard-to-read expression - I've laid it out here:
				input as T ??                           // Simple, if result is T then return result as T.
				((input is CompoundResult cr            // If it's not T: Is it a compound result...
				&& cr.IndividualResults.CountEquals(1)) //  with only one item?
				? cr.IndividualResults.First() as T     //   Then return the first (and only) item as T, returning null if it's not T.
				: null);                                // otherwise return null.
			return result != null;
		}
	}
}
