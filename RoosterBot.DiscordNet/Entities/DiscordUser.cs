using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordUser : IUser {
		public Discord.IUser DiscordEntity { get; }
		public PlatformComponent Platform => DiscordNetComponent.Instance;

		public object Id => DiscordEntity.Id;
		public string UserName => DiscordEntity.Username + "#" + DiscordEntity.Discriminator;
		public string DisplayName => (DiscordEntity is Discord.IGuildUser igu) ? igu.Nickname : DiscordEntity.Username;
		public string Mention => DiscordEntity.Mention;

		public async Task<IChannel?> GetPrivateChannel() => new DiscordChannel(await DiscordEntity.GetOrCreateDMChannelAsync());

		internal DiscordUser(Discord.IUser discordUser) {
			DiscordEntity = discordUser;
		}
	}
}
