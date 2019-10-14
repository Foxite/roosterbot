using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public sealed class GuildConfigService {
		private readonly ConfigService m_Config;
		private readonly IDiscordClient m_Client;
		private Provider m_Provider;

		internal GuildConfigService(ConfigService config, IDiscordClient client) {
			m_Config = config;
			m_Client = client;
		}

		private GuildConfig GetDefaultConfig(IGuild guild) {
			return new GuildConfig(guild) {
				Culture = m_Config.DefaultCulture, // TODO discord is going to add a preferred locale to guilds, we should use that as soon as Discord.NET releases support for it
				CommandPrefix = m_Config.DefaultCommandPrefix
			};
		}

		private GuildConfig GetDefaultConfig(ulong guildId) {
			return new GuildConfig(m_Client, guildId) {
				Culture = m_Config.DefaultCulture,
				CommandPrefix = m_Config.DefaultCommandPrefix
			};
		}

		public void InstallProvider(Provider provider) {
			if (m_Provider != null) {
				throw new InvalidOperationException("A provider is already installed: " + m_Provider.GetType().FullName);
			}
			m_Provider = provider;
		}

		public Task<bool> UpdateGuildAsync(GuildConfig config) {
			return m_Provider.UpdateGuildAsync(config);
		}

		public async Task<GuildConfig> GetConfigAsync(IGuild guild) {
			GuildConfig ret;
			if (m_Provider == null) {
				ret = GetDefaultConfig(guild);
			} else {
				ret = await m_Provider.GetGuildAsync(guild);
				if (ret == null) {
					await m_Provider.UpdateGuildAsync(ret);
				}
			}
			return ret;
		}

		public async Task<GuildConfig> GetConfigAsync(ulong guildId) {
			GuildConfig ret;
			if (m_Provider == null) {
				ret = GetDefaultConfig(guildId);
			} else {
				ret = await m_Provider.GetGuildAsync(guildId);
				if (ret == null) {
					await m_Provider.UpdateGuildAsync(ret);
				}
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
			public virtual Task<GuildConfig> GetGuildAsync(IGuild guild) => GetGuildAsync(guild.Id);

			/// <summary>
			/// Returns null if the guild is unknown.
			/// </summary>
			public abstract Task<GuildConfig> GetGuildAsync(ulong guildId);

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
		public Lazy<IGuild> Guild { get; }

		public GuildConfig(IDiscordClient client, ulong guildId) {
			GuildId = guildId;
			Guild = new Lazy<IGuild>(() => {
				return client.GetGuildAsync(guildId).GetAwaiter().GetResult();
			});
		}

		public GuildConfig(IGuild guild) {
			GuildId = guild.Id;
			Guild = new Lazy<IGuild>(guild);
		}
	}
}
