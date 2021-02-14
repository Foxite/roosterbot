using System;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RoosterBot.Meta {
	internal class EFChannelConfigService : ChannelConfigService {
		private readonly DatabaseProvider m_DbProvider;

		public EFChannelConfigService(DatabaseProvider dbProvider, string defaultCommandPrefix, CultureInfo defaultCulture) : base(defaultCommandPrefix, defaultCulture) {
			m_DbProvider = dbProvider;
		}

		public async override Task<ChannelConfig> GetConfigAsync(SnowflakeReference channel) {
			await using var ctx = new EFContext(m_DbProvider);

			EFChannel? result = await ctx.Channels
				.Where(config => config.Platform == channel.Platform.PlatformName && config.PlatformId == channel.Id.ToString())
				.SingleOrDefaultAsync();

			return result?.ToRealConfig(this) ?? GetDefaultConfig(channel);
		}

		public async override Task UpdateChannelAsync(ChannelConfig config) {
			await using var ctx = new EFContext(m_DbProvider);
			var efc = EFChannel.FromRealConfig(config);
			// BAD! BAD! BAD!
			// Only for testing. This is going to cause concurrency errors one day.
			// TODO Find a way to use SQL transactions under EF
			if (ctx.Channels.Any(item => item.Platform == config.ChannelReference.Platform.PlatformName && item.PlatformId == config.ChannelReference.Id.ToString())) {
				ctx.Channels.Update(efc);
			} else {
				ctx.Channels.Add(efc);
			}
			await ctx.SaveChangesAsync();
		}
	}
}
