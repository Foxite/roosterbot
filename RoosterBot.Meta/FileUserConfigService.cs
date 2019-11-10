using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class FileUserConfigService : UserConfigService {
		private readonly string m_ConfigPath;
		private IDictionary<ulong, UserConfig> m_Configs;

		public FileUserConfigService(string configPath) {
			IDictionary<string, JToken> jsonConfig = JObject.Parse(File.ReadAllText(configPath));
			m_Configs = jsonConfig.ToDictionary(
				/* Key */ kvp => ulong.Parse(kvp.Key),
				/* Val */ kvp => {
					JObject userJO = kvp.Value.ToObject<JObject>();
					string cultureString = userJO["culture"].ToObject<string>();
					return new UserConfig(
						this,
						cultureString.Length == 0 ? CultureInfo.GetCultureInfo(cultureString) : null,
						ulong.Parse(kvp.Key),
						userJO["customData"].ToObject<JObject>()
					);
				}
			);
			m_ConfigPath = configPath;
		}

		public override Task<UserConfig> GetConfigAsync(IUser user) {
			if (!m_Configs.TryGetValue(user.Id, out UserConfig? gc)) {
				gc = GetDefaultConfig(user.Id);
			}
			
			return Task.FromResult(gc);
		}

		public override Task UpdateUserAsync(UserConfig config) {
			m_Configs[config.UserId] = config;
			// TODO (fix) this doesn't work because it doesn't know how to serialize UserConfig
			File.WriteAllText(m_ConfigPath, JObject.FromObject(m_Configs).ToString(Newtonsoft.Json.Formatting.None));
			return Task.CompletedTask;
		}
	}
}
