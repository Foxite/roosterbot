using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RoosterBot.Meta {
	internal class EFUserConfigService : UserConfigService {
		private readonly DatabaseProvider m_DbProvider;

		public EFUserConfigService(DatabaseProvider dbProvider) {
			m_DbProvider = dbProvider;
		}

		public async override Task<UserConfig> GetConfigAsync(SnowflakeReference user) {
			await using var ctx = new EFContext(m_DbProvider);

			EFUser? result = await ctx.Users
				.Where(config => config.Platform == user.Platform.PlatformName && config.PlatformId == user.Id.ToString())
				.SingleOrDefaultAsync();

			return result?.ToRealConfig(this) ?? GetDefaultConfig(user);
		}

		public async override Task UpdateUserAsync(UserConfig config) {
			await using var ctx = new EFContext(m_DbProvider);
			var efu = EFUser.FromRealConfig(config);
			// BAD! BAD! BAD!
			// Only for testing. This is going to cause concurrency errors one day.
			// TODO Find a way to use SQL transactions under EF
			if (ctx.Users.Any(item => item.Platform == config.UserReference.Platform.PlatformName && item.PlatformId == config.UserReference.Id.ToString())) {
				ctx.Users.Update(efu);
			} else {
				ctx.Users.Add(efu);
			}
			await ctx.SaveChangesAsync();
		}
	}
}
