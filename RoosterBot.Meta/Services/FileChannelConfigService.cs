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
	public class FileChannelConfigService : ChannelConfigService {
		private readonly string m_ConfigFilePath;
		private readonly ConcurrentDictionary<SnowflakeReference, ChannelConfig> m_ConfigMap;

		public FileChannelConfigService(string configPath, string defaultCommandPrefix, CultureInfo defaultCulture) : base(defaultCommandPrefix, defaultCulture) {
			Logger.Info("FileChannelConfigService", "Loading channel config json");

			m_ConfigFilePath = configPath;
			m_ConfigMap = new ConcurrentDictionary<SnowflakeReference, ChannelConfig>();

			if (File.Exists(m_ConfigFilePath)) {
				var jsonConfig = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, FileChannelConfig>>>(File.ReadAllText(m_ConfigFilePath));

				foreach (KeyValuePair<string, IDictionary<string, FileChannelConfig>> platformKvp in jsonConfig) {
					PlatformComponent? platform = Program.Instance.Components.GetPlatform(platformKvp.Key);
					if (platform is null) {
						continue;
					}

					foreach (KeyValuePair<string, FileChannelConfig> configItem in platformKvp.Value) {
						var channelRef = new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(configItem.Key));
						m_ConfigMap.TryAdd(
							channelRef,
							new ChannelConfig(
								this,
								configItem.Value.CommandPrefix,
								CultureInfo.GetCultureInfo(configItem.Value.Culture),
								channelRef,
								configItem.Value.CustomData,
								configItem.Value.DisabledModules
							)
						);
					}
				}
			} else {
				using var sw = File.CreateText(m_ConfigFilePath);
				sw.Write("{}");
			}

			Logger.Info("FileChannelConfigService", "Finished loading channel config json");
		}

		public override Task<ChannelConfig> GetConfigAsync(SnowflakeReference channel) {
			return Task.FromResult(m_ConfigMap.GetOrAdd(channel, GetDefaultConfig));
		}

		public override Task UpdateChannelAsync(ChannelConfig config) {
			m_ConfigMap.AddOrUpdate(config.ChannelReference, config, (channel, old) => config);

			return File.WriteAllTextAsync(m_ConfigFilePath, JObject.FromObject(
				m_ConfigMap.GroupBy(kvp => kvp.Key.Platform.PlatformName).ToDictionary(
					grp => grp.Key,
					grp => JObject.FromObject(grp.ToDictionary(
						kvp => kvp.Key.Id.ToString() ?? "null",
						kvp => new FileChannelConfig() {
							Culture = kvp.Value.Culture.Name,
							CommandPrefix = kvp.Value.CommandPrefix,
							CustomData = (kvp.Value.GetRawData() as IDictionary<string, JToken?>).ToDictionary(
								innerKvp => innerKvp.Key,
								innerKvp => innerKvp.Value ?? (JToken) JValue.CreateNull()
							),
							DisabledModules = kvp.Value.DisabledModules
						}
					))
				)
			).ToString(Formatting.None));
		}

		private class FileChannelConfig {
			public string Culture { get; set; } = null!;
			public string CommandPrefix { get; set; } = null!;
			public IDictionary<string, JToken> CustomData { get; set; } = null!;
			public IEnumerable<string> DisabledModules { get; set; } = null!;
		}
	}
}
