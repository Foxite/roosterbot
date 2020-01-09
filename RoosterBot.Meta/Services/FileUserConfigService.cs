﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class FileUserConfigService : UserConfigService {
		private readonly string m_ConfigFilePath;
		private readonly ConcurrentDictionary<SnowflakeReference, UserConfig> m_ConfigMap;

		public FileUserConfigService(string configPath) {
			Logger.Info("FileUserConfigService", "Loading user config json");

			m_ConfigFilePath = configPath;

			var jsonConfig = JObject.Parse(File.ReadAllText(m_ConfigFilePath)).ToObject<IDictionary<string, IDictionary<string, FileUserConfig>>>();
			m_ConfigMap = new ConcurrentDictionary<SnowflakeReference, UserConfig>();

			if (jsonConfig is null) {
				throw new FormatException("Guild config map could not be deserialized."); // TODO useful error message
			}

			foreach (KeyValuePair<string, IDictionary<string, FileUserConfig>> platformKvp in jsonConfig) {
				PlatformComponent? platform = Program.Instance.Components.GetPlatform(platformKvp.Key);
				if (platform is null) {
					// TODO optional skip instead of crash
					throw new KeyNotFoundException("No PlatformComponent for `" + platformKvp.Key + "` is installed.");
				}

				foreach (KeyValuePair<string, FileUserConfig> configItem in platformKvp.Value) {
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

			Logger.Info("FileUserConfigService", "Finished loading user config json");
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
						kvp => new FileUserConfig() {
							Culture = kvp.Value.Culture?.Name ?? null,
							CustomData = (kvp.Value.GetRawData() as IDictionary<string, JToken?>).ToDictionary(
								innerKvp => innerKvp.Key,
								innerKvp => innerKvp.Value ?? (JToken) JValue.CreateNull()
							)
						}
					))
				)
			).ToString(Formatting.None));
		}

		private class FileUserConfig {
			[JsonProperty("culture")]
			public string? Culture { get; set; } = null!;

			[JsonProperty("customData")]
			public IDictionary<string, JToken> CustomData { get; set; } = null!;
		}
	}
}
