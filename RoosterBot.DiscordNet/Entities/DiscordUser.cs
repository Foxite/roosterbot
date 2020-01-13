namespace RoosterBot.DiscordNet {
	public class DiscordUser : IUser {
		public Discord.IUser DiscordEntity { get; }
		public PlatformComponent Platform => DiscordNetComponent.Instance;

		object ISnowflake.Id => DiscordEntity.Id;
		public ulong  Id     => DiscordEntity.Id;
		public string UserName => DiscordEntity.Username + "#" + DiscordEntity.Discriminator;
		public string DisplayName => (DiscordEntity is Discord.IGuildUser igu) ? igu.Nickname : DiscordEntity.Username;
		public string Mention => DiscordEntity.Mention;
		public bool IsBotAdmin => Id == DiscordNetComponent.Instance.BotOwnerId;
		public bool IsRoosterBot => DiscordNetComponent.Instance.Client.CurrentUser.Id == Id;

		internal DiscordUser(Discord.IUser discordUser) {
			DiscordEntity = discordUser;
		}

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
