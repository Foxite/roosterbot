using System.Globalization;
using System.Linq;
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

		public override Task UpdateChannelAsync(ChannelConfig config) => Task.Run(() => {
			using var ctx = new EFContext(m_DbProvider);
			ctx.Channels.Update(EFChannel.FromRealConfig(config));
		});
	}
}
