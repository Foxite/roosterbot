using System.Linq;
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

		public override Task UpdateUserAsync(UserConfig config) => Task.Run(() => {
			using var ctx = new EFContext(m_DbProvider);
			ctx.Users.Update(EFUser.FromRealConfig(config));
		});
	}
}
