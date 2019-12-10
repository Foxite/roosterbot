using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public abstract class GuildConfigService {
		private readonly ConfigService m_Config;

		protected GuildConfigService(ConfigService config) {
			m_Config = config;
		}

		public GuildConfig GetDefaultConfig() {
			return GetDefaultConfig(0);
		}

		protected GuildConfig GetDefaultConfig(ulong guildId) {
			return new GuildConfig(this, m_Config.DefaultCommandPrefix, m_Config.DefaultCulture, guildId, new Dictionary<string, JToken>());
		}

		public abstract Task UpdateGuildAsync(GuildConfig config);
		public abstract Task<GuildConfig> GetConfigAsync(IGuild guild);
	}

	/// <summary>
	/// This class stores data about a guild's preferences with this bot. Built-in are the language and command prefix, and it can store arbitrary custom data.
	/// 
	/// This arbitrary data is serialized using Newtonsoft.Json. This means most built-in .NET types will work, and any custom data structure that you store must
	/// be serializable with Newtonsoft.Json. Read its documentation to learn more.
	/// </summary>
	public class GuildConfig {
		private GuildConfigService m_Service;
		private IDictionary<string, JToken> m_CustomData;

		public string CommandPrefix { get; set; }
		public CultureInfo Culture { get; set; }
		public ulong GuildId { get; }

		public GuildConfig(GuildConfigService guildConfigService, string commandPrefix, CultureInfo culture, ulong guildId, IDictionary<string, JToken> customData) {
			m_Service = guildConfigService;
			CommandPrefix = commandPrefix;
			Culture = culture;
			GuildId = guildId;
			m_CustomData = customData;
		}

		public bool TryGetData<T>(string key, [MaybeNullWhen(false)] out T data, T defaultValue = default) {
			if (m_CustomData.TryGetValue(key, out JToken? value)) {
				data = value.ToObject<T>();
				return true;
			} else {
				data = defaultValue;
				return false;
			}
		}

		public void SetData<T>(string key, T data) {
			m_CustomData[key] = JToken.FromObject(data);
		}

		public Task UpdateAsync() {
			return m_Service.UpdateGuildAsync(this);
		}

		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
