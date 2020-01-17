using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public abstract class ChannelConfigService {
		private readonly GlobalConfigService m_Config;

		protected ChannelConfigService(GlobalConfigService config) {
			m_Config = config;
		}

		protected ChannelConfig GetDefaultConfig(SnowflakeReference channel) {
			return new ChannelConfig(this, m_Config.DefaultCommandPrefix, m_Config.DefaultCulture, channel, new Dictionary<string, JToken>());
		}

		public abstract Task UpdateChannelAsync(ChannelConfig config);
		public abstract Task<ChannelConfig> GetConfigAsync(SnowflakeReference channel);
	}

	/// <summary>
	/// This class stores data about a channel's preferences with this bot. Built-in are the language and command prefix, and it can store arbitrary custom data.
	/// 
	/// This arbitrary data is serialized using Newtonsoft.Json. This means most built-in .NET types will work, and any custom data structure that you store must
	/// be serializable with Newtonsoft.Json. Read its documentation to learn more.
	/// </summary>
	public class ChannelConfig {
		private readonly ChannelConfigService m_Service;
		private readonly IDictionary<string, JToken> m_CustomData;

		public SnowflakeReference ChannelReference { get; }
		public string CommandPrefix { get; set; }
		public CultureInfo Culture { get; set; }

		public ChannelConfig(ChannelConfigService service, string commandPrefix, CultureInfo culture, SnowflakeReference channel, IDictionary<string, JToken> customData) {
			m_Service = service;
			CommandPrefix = commandPrefix;
			Culture = culture;
			m_CustomData = customData;
			ChannelReference = channel;
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
			if (data is null) {
				m_CustomData.Remove(key);
			} else {
				m_CustomData[key] = JToken.FromObject(data);
			}
		}

		public Task UpdateAsync() {
			return m_Service.UpdateChannelAsync(this);
		}

		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
