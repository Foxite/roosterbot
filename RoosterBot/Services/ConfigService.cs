using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public sealed class ConfigService {
		public string DefaultCommandPrefix { get; }
		public CultureInfo DefaultCulture { get; }

		internal ConfigService(string jsonPath) {
			if (!Directory.Exists(Program.DataPath)) {
				throw new DirectoryNotFoundException("Data folder did not exist.");
			}

			if (!File.Exists(jsonPath)) {
				throw new FileNotFoundException("Config file did not exist.");
			}

			string jsonFile = File.ReadAllText(jsonPath);
			var jsonConfig = JObject.Parse(jsonFile);

			DefaultCommandPrefix = jsonConfig["defaultCommandPrefix"]!.ToObject<string>()!;
			DefaultCulture       = CultureInfo.GetCultureInfo(jsonConfig["defaultCulture"]!.ToObject<string>()!);
		}
	}
}
