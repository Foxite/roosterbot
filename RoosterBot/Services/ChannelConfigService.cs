using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	/// <summary>
	/// The base class for a service providing configuration for <see cref="IChannel"/>s.
	/// </summary>
	public abstract class ChannelConfigService {
		private readonly GlobalConfigService m_Config;

		/// <summary>
		/// Construct an instance of <see cref="ChannelConfigService"/>.
		/// </summary>
		protected ChannelConfigService(GlobalConfigService config) {
			m_Config = config;
		}

		/// <summary>
		/// Get the default config for a channel represented by a <see cref="SnowflakeReference"/>.
		/// </summary>
		protected ChannelConfig GetDefaultConfig(SnowflakeReference channel) {
			return new ChannelConfig(this, m_Config.DefaultCommandPrefix, m_Config.DefaultCulture, channel, new Dictionary<string, JToken>());
		}

		/// <summary>
		/// Save the changes applied to a <see cref="ChannelConfig"/> object.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public abstract Task UpdateChannelAsync(ChannelConfig config);

		/// <summary>
		/// Get the <see cref="ChannelConfig"/> for a channel represented by a <see cref="SnowflakeReference"/>.
		/// </summary>
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

		/// <summary>
		/// The reference to the <see cref="IChannel"/> this object pertains to.
		/// </summary>
		public SnowflakeReference ChannelReference { get; }

		/// <summary>
		/// The command prefix used in the channel.
		/// </summary>
		public string CommandPrefix { get; set; }

		/// <summary>
		/// The culture used in the channel.
		/// </summary>
		public CultureInfo Culture { get; set; }

		/// <summary>
		/// Construct a new instance of the ChannelConfig class.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="commandPrefix"></param>
		/// <param name="culture"></param>
		/// <param name="channel"></param>
		/// <param name="customData"></param>
		public ChannelConfig(ChannelConfigService service, string commandPrefix, CultureInfo culture, SnowflakeReference channel, IDictionary<string, JToken> customData) {
			m_Service = service;
			CommandPrefix = commandPrefix;
			Culture = culture;
			m_CustomData = customData;
			ChannelReference = channel;
		}

		/// <summary>
		/// Try to get the value of a configuration key and convert it to <typeparamref name="T"/>.
		/// </summary>
		public bool TryGetData<T>(string key, [MaybeNullWhen(false)] out T data, T defaultValue = default) {
			if (m_CustomData.TryGetValue(key, out JToken? value)) {
				data = value.ToObject<T>();
				return true;
			} else {
				data = defaultValue;
				return false;
			}
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
		/// <returns></returns>
		public Task UpdateAsync() {
			return m_Service.UpdateChannelAsync(this);
		}

		/// <summary>
		/// Get the full Json data for this object.
		/// </summary>
		/// <returns></returns>
		public JObject GetRawData() => JObject.FromObject(m_CustomData);
	}
}
