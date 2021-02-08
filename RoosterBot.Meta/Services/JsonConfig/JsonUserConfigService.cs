using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	internal class JsonUserConfigService : UserConfigService {
		private readonly string m_ConfigFilePath;
		private readonly ConcurrentDictionary<SnowflakeReference, UserConfig> m_ConfigMap;

		public JsonUserConfigService(string configPath) {
			Logger.Info(MetaComponent.LogTag, "Loading user config json");

			m_ConfigFilePath = configPath;
			m_ConfigMap = new ConcurrentDictionary<SnowflakeReference, UserConfig>();

			if (File.Exists(m_ConfigFilePath)) {
				var jsonConfig = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, JsonUserConfig>>>(File.ReadAllText(m_ConfigFilePath));

				foreach (KeyValuePair<string, IDictionary<string, JsonUserConfig>> platformKvp in jsonConfig) {
					PlatformComponent? platform = Program.Instance.Components.GetPlatform(platformKvp.Key);
					if (platform is null) {
						continue;
					}

					foreach (KeyValuePair<string, JsonUserConfig> configItem in platformKvp.Value) {
						var userRef = new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(configItem.Key));
						m_ConfigMap.TryAdd(
							userRef,
							new UserConfig(
								this,
								configItem.Value.Culture is null ? null : CultureInfo.GetCultureInfo(configItem.Value.Culture),
								userRef,
								configItem.Value.CustomData
							)
						);
					}
				}
			} else {
				using var sw = File.CreateText(m_ConfigFilePath);
				sw.Write("{}");
			}
			Logger.Info(MetaComponent.LogTag, "Finished loading user config json");
		}

		public override Task<UserConfig> GetConfigAsync(SnowflakeReference user) {
			return Task.FromResult(m_ConfigMap.GetOrAdd(user, GetDefaultConfig));
		}

		public override Task UpdateUserAsync(UserConfig config) {
			m_ConfigMap.AddOrUpdate(config.UserReference, config, (channel, old) => config);

			return File.WriteAllTextAsync(m_ConfigFilePath, JObject.FromObject(
				m_ConfigMap.GroupBy(kvp => kvp.Key.Platform.PlatformName).ToDictionary(
					grp => grp.Key,
					grp => JObject.FromObject(grp.ToDictionary(
						kvp => kvp.Key.Id.ToString() ?? "null",
						kvp => new JsonUserConfig() {
							Culture = kvp.Value.Culture?.Name ?? null,
							CustomData = (kvp.Value.GetRawData() as IDictionary<string, JToken?>).ToDictionary(
								innerKvp => innerKvp.Key,
								innerKvp => innerKvp.Value ?? JValue.CreateNull()
							)
						}
					))
				)
			).ToString(Formatting.None));
		}

		private class JsonUserConfig {
			public string? Culture { get; set; } = null!;
			public IDictionary<string, JToken> CustomData { get; set; } = null!;
		}
	}
}
