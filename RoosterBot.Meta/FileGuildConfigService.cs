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
				/* Val */ kvp => {
					return new GuildConfig(this,
						kvp.Value["commandPrefix"].ToObject<string>(),
						CultureInfo.GetCultureInfo(kvp.Value["culture"].ToObject<string>()),
						ulong.Parse(kvp.Key),
						kvp.Value.ToObject<JObject>()["customData"].ToObject<JObject>()
					);
				}
			);
		}

		public override Task<GuildConfig> GetConfigAsync(IGuild guild) {
			return Task.FromResult(m_Configs[guild.Id]);
		}

		public async override Task UpdateGuildAsync(GuildConfig config) {
			m_Configs[config.GuildId] = config;
			await File.WriteAllTextAsync(m_ConfigFilePath, JObject.FromObject(m_Configs).ToString(Newtonsoft.Json.Formatting.None));
		}
	}
}
