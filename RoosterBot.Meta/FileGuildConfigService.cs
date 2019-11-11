using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class FileGuildConfigService : GuildConfigService {
		private readonly string m_ConfigFilePath;
		private readonly IDictionary<ulong, GuildConfig> m_Configs;

		public FileGuildConfigService(ConfigService config, string configPath) : base(config) {
			m_ConfigFilePath = configPath;

			// Read config file and populate m_Configs
			IDictionary<string, JToken> jsonConfig = JObject.Parse(File.ReadAllText(m_ConfigFilePath));
			m_Configs = jsonConfig.ToDictionary(
				/* Key */ kvp => ulong.Parse(kvp.Key),
				/* Val */ kvp => new GuildConfig(this,
					kvp.Value["commandPrefix"].ToObject<string>(),
					CultureInfo.GetCultureInfo(kvp.Value["culture"].ToObject<string>()),
					ulong.Parse(kvp.Key),
					kvp.Value.ToObject<JObject>()["customData"].ToObject<JObject>()
				)
			);
		}

		public override Task<GuildConfig> GetConfigAsync(IGuild guild) {
			if (!m_Configs.TryGetValue(guild.Id, out GuildConfig? gc)) {
				gc = GetDefaultConfig(guild.Id);
			}
			
			return Task.FromResult(gc);
		}

		public override Task UpdateGuildAsync(GuildConfig config) {
			m_Configs[config.GuildId] = config;
			JObject jsonConfig = new JObject();

			foreach (KeyValuePair<ulong, GuildConfig> kvp in m_Configs) {
				jsonConfig[kvp.Key.ToString()] = new JObject {
					["culture"] = kvp.Value.Culture.Name,
					["commandPrefix"] = kvp.Value.CommandPrefix,
					["customData"] = kvp.Value.GetRawData()
				};
			}

			return File.WriteAllTextAsync(m_ConfigFilePath, jsonConfig.ToString(Newtonsoft.Json.Formatting.None));
		}
	}
}
