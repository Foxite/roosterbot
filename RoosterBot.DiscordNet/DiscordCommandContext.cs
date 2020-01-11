using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	public class DiscordCommandContext : RoosterCommandContext {
		public BaseSocketClient Client { get; }
		public new Discord.IUserMessage Message { get; }
		public new Discord.IUser User { get; }
		public new Discord.IMessageChannel Channel { get; }
		public Discord.IGuild? Guild { get; }

		public DiscordCommandContext(DiscordMessage message, UserConfig userConfig, ChannelConfig guildConfig) : base(DiscordNetComponent.Instance, message, userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message.DiscordEntity;
			User = Message.Author;
			Channel = Message.Channel;
			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}
	}
}
