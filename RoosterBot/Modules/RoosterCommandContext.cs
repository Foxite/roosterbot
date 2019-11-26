using System;
using System.Globalization;
using Discord;
using Qmmands;

namespace RoosterBot {
	public class RoosterCommandContext : CommandContext {
		public IDiscordClient Client { get; }
		public IUserMessage Message { get; }
		public IUser User { get; }
		public IMessageChannel Channel { get; }
		public IGuild? Guild { get; }
		public bool IsPrivate { get; }

		public UserConfig UserConfig { get; }
		public GuildConfig GuildConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? GuildConfig.Culture;

		public RoosterCommandContext(IDiscordClient client, IUserMessage message, UserConfig userConfig, GuildConfig guildConfig, IServiceProvider isp) : base(isp) {
			Client = client;
			Message = message;
			User = message.Author;
			Channel = message.Channel;
			IsPrivate = Channel is IPrivateChannel;
			Guild = (Channel as IGuildChannel)?.Guild;

			UserConfig = userConfig;
			GuildConfig = guildConfig;
		}

		public override string ToString() {
			if (Guild != null) {
				return $"{User.Username}#{User.Discriminator} in `{Guild.Name}` channel `{Channel.Name}`: {Message.Content}";
			} else {
				return $"{User.Username}#{User.Discriminator} in private channel `{Channel.Name}`: {Message.Content}";
			}
		}
	}
}
