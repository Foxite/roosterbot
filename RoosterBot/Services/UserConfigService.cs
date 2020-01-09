using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	/// <summary>
	/// Provides user-specific overrides for the GuildConfigService.
	/// </summary>
	public abstract class UserConfigService {
		public abstract Task<UserConfig> GetConfigAsync(SnowflakeReference user);
		public abstract Task UpdateUserAsync(UserConfig config);

		protected UserConfig GetDefaultConfig(SnowflakeReference user) {
			return new UserConfig(this, null, user, new Dictionary<string, JToken>());
		}
	}

	public class UserConfig {
		private readonly UserConfigService m_Service;
		private readonly IDictionary<string, JToken> m_CustomData;

		public SnowflakeReference UserReference { get; }
		public CultureInfo? Culture { get; set; }

		public UserConfig(UserConfigService userConfigService, CultureInfo? culture, SnowflakeReference user, IDictionary<string, JToken> customData) {
			m_Service = userConfigService;
			Culture = culture;
			UserReference = user;
			m_CustomData = customData;
		}

		public bool TryGetData<T>(string key, [MaybeNullWhen(false), NotNullWhen(true)] out T data, T defaultValue = default) {
			if (m_CustomData.TryGetValue(key, out JToken? value)) {
				data = value.ToObject<T>();
				return true;
			} else {
				data = defaultValue;
				return false;
			}
		}

		public void SetData<T>(string key, T data) {
			if (data is null) {
				m_CustomData.Remove(key);
			} else {
				m_CustomData[key] = JToken.FromObject(data);
			}
		}

		public Task UpdateAsync() {
			return m_Service.UpdateUserAsync(this);
		}

		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
