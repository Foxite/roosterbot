using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public sealed class GuildConfigService {
		private readonly ConfigService m_Config;
		private Provider? m_Provider;

		internal GuildConfigService(ConfigService config) {
			m_Config = config;
		}

		public GuildConfig GetDefaultConfig() {
			return GetDefaultConfig(0);
		}

		private GuildConfig GetDefaultConfig(ulong guildId) {
			return new GuildConfig(guildId, m_Config.DefaultCulture, m_Config.DefaultCommandPrefix);
		}

		public void InstallProvider(Provider provider) {
			if (m_Provider != null) {
				throw new InvalidOperationException("A provider is already installed: " + m_Provider.GetType().FullName);
			}
			m_Provider = provider;
		}

		public Task<bool> UpdateGuildAsync(GuildConfig config) {
			if (m_Provider == null) {
				return Task.FromResult(false);
			} else {
				return m_Provider.UpdateGuildAsync(config);
			}
		}

		public Task<GuildConfig?> GetConfigAsync(IGuild guild) => GetConfigAsync(guild.Id);

		public async Task<GuildConfig?> GetConfigAsync(ulong guildId) {
			GuildConfig? ret;
			if (m_Provider == null) {
				ret = GetDefaultConfig(guildId);
			} else {
				ret = await m_Provider.GetGuildAsync(guildId);
			}
			return ret;
		}

		/// <summary>
		/// Provides GuildConfigService with its data.
		/// </summary>
		public abstract class Provider {
			/// <summary>
			/// Returns null if the guild is unknown.
			/// </summary>
			public virtual Task<GuildConfig?> GetGuildAsync(IGuild guild) => GetGuildAsync(guild.Id);

			/// <summary>
			/// Returns null if the guild is unknown.
			/// </summary>
			public abstract Task<GuildConfig?> GetGuildAsync(ulong guildId);

			/// <summary>
			/// Returns a value representing the success of the operation.
			/// </summary>
			public abstract Task<bool> UpdateGuildAsync(GuildConfig config);

			/// <summary>
			/// Enumerate all known guilds.
			/// </summary>
			public abstract IEnumerator<GuildConfig> GetEnumerator();
		}
	}

	public class GuildConfig {
		public CultureInfo Culture { get; set; }
		public string CommandPrefix { get; set; }

		public ulong GuildId { get; }

		public GuildConfig(ulong guildId, CultureInfo culture, string commandPrefix) {
			GuildId = guildId;
			Culture = culture;
			CommandPrefix = commandPrefix;
		}
	}
}
