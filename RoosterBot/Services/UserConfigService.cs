using Discord;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Provides user-specific overrides for the GuildConfigService.
	/// </summary>
	public abstract class UserConfigService {
		public abstract Task<UserConfig> GetConfigAsync(IUser user);
		public abstract Task UpdateUserAsync(UserConfig config);

		protected UserConfig GetDefaultConfig(ulong id) {
			return new UserConfig(this, null, id, new Dictionary<string, JToken>());
		}
	}

	public class UserConfig {
		private UserConfigService m_Service;
		private IDictionary<string, JToken> m_CustomData;

		public ulong UserId { get; }
		public CultureInfo? Culture { get; set; }

		public UserConfig(UserConfigService userConfigService, CultureInfo? culture, ulong userId, IDictionary<string, JToken> customData) {
			m_Service = userConfigService;
			Culture = culture;
			UserId = userId;
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
			return m_Service.UpdateUserAsync(this);
		}

		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
