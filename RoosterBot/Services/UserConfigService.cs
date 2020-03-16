using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	/// <summary>
	/// Provides user-specific overrides for the <see cref="ChannelConfigService"/>.
	/// </summary>
	public abstract class UserConfigService {
		/// <summary>
		/// Get the config for an <see cref="IUser"/> represented by a <see cref="SnowflakeReference"/>
		/// </summary>
		public abstract Task<UserConfig> GetConfigAsync(SnowflakeReference user);

		/// <summary>
		/// Save the changes applied to a <see cref="UserConfig"/> object.
		/// </summary>
		public abstract Task UpdateUserAsync(UserConfig config);

		/// <summary>
		/// Get the default configuration for an <see cref="IUser"/> represented by a <see cref="SnowflakeReference"/>
		/// </summary>
		protected UserConfig GetDefaultConfig(SnowflakeReference user) {
			return new UserConfig(this, null, user, new Dictionary<string, JToken>());
		}
	}

	/// <summary>
	/// The configuration for an <see cref="IUser"/>.
	/// </summary>
	public class UserConfig {
		private readonly UserConfigService m_Service;
		private readonly IDictionary<string, JToken> m_CustomData;

		/// <summary>
		/// The <see cref="SnowflakeReference"/> representing the <see cref="IUser"/> that this object pertains to.
		/// </summary>
		public SnowflakeReference UserReference { get; }

		/// <summary>
		/// The culture this user has configured for themselves. If <see langword="null"/>, then the <see cref="ChannelConfig.Culture"/> should be used.
		/// </summary>
		public CultureInfo? Culture { get; set; }

		/// <summary>
		/// Construct a new instance of UserConfig.
		/// </summary>
		public UserConfig(UserConfigService userConfigService, CultureInfo? culture, SnowflakeReference user, IDictionary<string, JToken> customData) {
			m_Service = userConfigService;
			Culture = culture;
			UserReference = user;
			m_CustomData = customData;
		}

		/// <summary>
		/// Try to get the value of a configuration key and convert it to <typeparamref name="T"/>.
		/// </summary>
		public bool TryGetData<T>(string key, [MaybeNullWhen(false), NotNullWhen(true)] out T data, T defaultValue = default) {
			if (m_CustomData.TryGetValue(key, out JToken? value)) {
				data = value.ToObject<T>();
				return true;
			} else {
				data = defaultValue;
				return false;
			}
		}

		/// <summary>
		/// Remove a key (and its value) from the configuration.
		/// </summary>
		public void RemoveData(string key) {
			m_CustomData.Remove(key);
		}

		/// <summary>
		/// Set the value of a configuration key.
		/// </summary>
		public void SetData<T>(string key, T data) {
			if (data is null) {
				m_CustomData.Remove(key);
			} else {
				m_CustomData[key] = JToken.FromObject(data);
			}
		}

		/// <summary>
		/// Save the changes applied to this object.
		/// </summary>
		public Task UpdateAsync() {
			return m_Service.UpdateUserAsync(this);
		}

		/// <summary>
		/// Get the full Json data for this object.
		/// </summary>
		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
