using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Meta {
	public class FileGuildConfigProvider : GuildConfigService.Provider {
		private IDictionary<ulong, GuildConfig> m_Guilds;
		private string m_FilePath;

		public FileGuildConfigProvider(string filePath) {
			m_FilePath = filePath;

			m_Guilds = new Dictionary<ulong, GuildConfig>();
			JObject jsonConfig = JObject.Parse(File.ReadAllText(filePath));
			foreach (KeyValuePair<string, JToken> kvp in jsonConfig) {
				ulong guildId = ulong.Parse(kvp.Key);
				m_Guilds.Add(guildId, new GuildConfig(guildId, CultureInfo.GetCultureInfo(kvp.Value["Culture"].ToObject<string>()), kvp.Value["CommandPrefix"].ToObject<string>()));
			}
		}

		public override IEnumerator<GuildConfig> GetEnumerator() => m_Guilds.Values.GetEnumerator();

		public override Task<GuildConfig> GetGuildAsync(ulong guildId) => Task.FromResult(m_Guilds[guildId]);

		public override Task<bool> UpdateGuildAsync(GuildConfig config) {
			m_Guilds[config.GuildId] = config;

			JObject jsonConfig = new JObject();
			foreach (KeyValuePair<ulong, GuildConfig> kvp in m_Guilds) {
				jsonConfig[kvp.Key.ToString()] = new JObject {
					["Culture"] = kvp.Value.Culture.Name,
					["CommandPrefix"] = kvp.Value.CommandPrefix
				};
			}
			File.WriteAllText(m_FilePath, jsonConfig.ToString());

			return Task.FromResult(true);
		}
	}
}
