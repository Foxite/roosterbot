using System;
using System.IO;
using Newtonsoft.Json;

namespace RoosterBot {
	public static class Util {
		public static readonly Random RNG = new Random();

		/// <summary>
		/// This will deserialize a Json file based on a template class. If the file does not exist, it will be created and the template will be serialized and written to the file.
		/// </summary>
		public static T LoadJsonConfigFromTemplate<T>(string filePath, T template, JsonSerializerSettings? jss = null) {
			if (File.Exists(filePath)) {
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), jss);
			} else {
				using var sw = File.CreateText(filePath);
				sw.Write(JsonConvert.SerializeObject(template));
				return template;
			}
		}
	}
}
