using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	// TODO actually start using this, add a database Provider via AWS
	public sealed class GuildConfigService {
		private readonly ConfigService m_Config;
		private readonly IDiscordClient m_Client;
		private readonly Provider m_Provider;

		internal GuildConfigService(ConfigService config, IDiscordClient client) {
			m_Config = config;
			m_Client = client;
		}

		public Task<bool> UpdateGuild(GuildConfig config) {
			return m_Provider.UpdateGuildAsync(config);
		}

		public async Task<GuildConfig> GetConfig(IGuild guild) {
			GuildConfig ret = await m_Provider.GetGuildAsync(guild);

			if (ret == null) {
				ret = new GuildConfig(guild) {
					Culture = m_Config.DefaultCulture, // TODO discord is going to add a preferred locale to guilds, we should use that as soon as Discord.NET releases support for it
					CommandPrefix = m_Config.DefaultCommandPrefix
				};
				await m_Provider.UpdateGuildAsync(ret);
			}
			return ret;
		}

		public async Task<GuildConfig> GetConfig(ulong guildId) {
			GuildConfig ret = await m_Provider.GetGuildAsync(guildId);

			if (ret == null) {
				ret = new GuildConfig(m_Client, guildId) {
					Culture = m_Config.DefaultCulture,
					CommandPrefix = m_Config.DefaultCommandPrefix
				};
				await m_Provider.UpdateGuildAsync(ret);
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
