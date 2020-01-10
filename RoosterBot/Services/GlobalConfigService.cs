using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace RoosterBot {
	public sealed class GlobalConfigService {
		/// <summary>
		/// The default command prefix for all channels.
		/// </summary>
		public string DefaultCommandPrefix { get; }

		/// <summary>
		/// The default culture for all channels.
		/// </summary>
		public CultureInfo DefaultCulture { get; }
		
		/// <summary>
		/// When loading config data, this controls if serialized SnowflakeReferences from unknown platforms are ignored, or cause an error.
		/// </summary>
		/// <remarks>
		/// RoosterBot does not use this. It is meant to be a single switch for all components that use serialized SnowflakeReferences in their configuration system.
		/// </remarks>
		public bool IgnoreUnknownPlatforms { get; }

		internal GlobalConfigService(string jsonPath) {
			if (!Directory.Exists(Program.DataPath)) {
				throw new DirectoryNotFoundException("Data folder did not exist.");
			}

			if (!File.Exists(jsonPath)) {
				throw new FileNotFoundException("Config file did not exist.");
			}

			var jsonConfig = JsonConvert.DeserializeAnonymousType(File.ReadAllText(jsonPath), new {
				DefaultCommandPrefix = "",
				DefaultCulture = "",
				IgnoreUnknownPlatforms = false
			});

			DefaultCommandPrefix = jsonConfig.DefaultCommandPrefix;
			DefaultCulture       = CultureInfo.GetCultureInfo(jsonConfig.DefaultCulture);
			IgnoreUnknownPlatforms = jsonConfig.IgnoreUnknownPlatforms;
		}
	}
}
