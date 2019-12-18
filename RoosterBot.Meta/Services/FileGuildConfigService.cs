using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class FileGuildConfigService : ChannelConfigService {
		private readonly string m_ConfigFilePath;
		private readonly IDictionary<object, ChannelConfig> m_Configs;

		public FileGuildConfigService(ConfigService config, string configPath) : base(config) {
			Logger.Info("FileGuildConfigService", "Loading guild config json");

			m_ConfigFilePath = configPath;

			// Read config file and populate m_Configs
			IDictionary<string, JToken> jsonConfig = JObject.Parse(File.ReadAllText(m_ConfigFilePath))!;
			m_Configs = jsonConfig.ToDictionary(
				/* Key */ kvp => (object) kvp.Key, // TODO doesn't atually work (see below)
				/* Val */ kvp => new ChannelConfig(this,
					kvp.Value["commandPrefix"]!.ToObject<string>()!,
					CultureInfo.GetCultureInfo(kvp.Value["culture"]!.ToObject<string>()!),
					kvp.Key, // See above
					kvp.Value.ToObject<JObject>()!["customData"]!.ToObject<JObject>()!
				)
			);

			Logger.Info("FileGuildConfigService", "Finished loading guild config json");
		}

		public override Task<ChannelConfig> GetConfigAsync(IChannel channel) {
			if (!m_Configs.TryGetValue(channel.Id, out ChannelConfig? gc)) {
				gc = GetDefaultConfig(channel.Id);
			}
			
			return Task.FromResult(gc);
		}

		public override Task UpdateGuildAsync(ChannelConfig config) {
			m_Configs[config.ChannelId] = config;
			var jsonConfig = new JObject();

			foreach (KeyValuePair<object, ChannelConfig> kvp in m_Configs) {
				jsonConfig[kvp.Key] = new JObject {
					["culture"] = kvp.Value.Culture.Name,
					["commandPrefix"] = kvp.Value.CommandPrefix,
					["customData"] = kvp.Value.GetRawData()
				};
			}

			return File.WriteAllTextAsync(m_ConfigFilePath, jsonConfig.ToString(Newtonsoft.Json.Formatting.None));
		}
	}
}
