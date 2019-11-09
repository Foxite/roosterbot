using System.Collections.Generic;
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

			// TODO read config file and populate m_Configs
		}

		public override Task<GuildConfig> GetConfigAsync(IGuild guild) {
			return Task.FromResult(m_Configs[guild.Id]);
		}

		public override Task<bool> UpdateGuildAsync(GuildConfig config) {
			// TODO write m_Configs back to file
		}
	}
}
