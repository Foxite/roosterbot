using System;
using System.Globalization;
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
			try {
				var jsonConfig = Util.LoadJsonConfigFromTemplate(jsonPath, new {
					DefaultCommandPrefix = "!",
					DefaultCulture = "en-US",
					IgnoreUnknownPlatforms = false
				});

				DefaultCommandPrefix = jsonConfig.DefaultCommandPrefix;
				DefaultCulture = CultureInfo.GetCultureInfo(jsonConfig.DefaultCulture);
				IgnoreUnknownPlatforms = jsonConfig.IgnoreUnknownPlatforms;
			} catch (JsonReaderException e) {
				throw new FormatException("Config.json contains invalid data.", e);
			}
		}
	}
}
