using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordUser : IUser {
		public Discord.IUser DiscordEntity { get; }
		public PlatformComponent Platform => DiscordNetComponent.Instance;

		public object Id => DiscordEntity.Id;
		public string UserName => DiscordEntity.Username + "#" + DiscordEntity.Discriminator;
		public string DisplayName => (DiscordEntity is Discord.IGuildUser igu) ? igu.Nickname : DiscordEntity.Username;
		public string Mention => DiscordEntity.Mention;
		public bool IsBotAdmin => false; // TODO bot admin id from config file

		internal DiscordUser(Discord.IUser discordUser) {
			DiscordEntity = discordUser;
		}

		public async Task<IChannel?> GetPrivateChannel() => new DiscordChannel(await DiscordEntity.GetOrCreateDMChannelAsync());

		public bool IsChannelAdmin(IChannel ic) {
			if (ic is DiscordChannel channel && DiscordEntity is Discord.IGuildUser user) {
				return user.GuildPermissions.Administrator
					|| user.GuildPermissions.ManageGuild
					|| user.GuildPermissions.KickMembers
					|| user.GuildPermissions.BanMembers;
			} else {
				return false;
			}
		}
	}
}
