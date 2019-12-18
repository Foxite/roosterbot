using System;
using System.Globalization;
using Qmmands;

namespace RoosterBot {
	public class RoosterCommandContext : CommandContext {
		public IMessage Message { get; }
		public IUser User { get; }
		public IChannel Channel { get; }
		public bool IsPrivate { get; }

		public UserConfig UserConfig { get; }
		public GuildConfig GuildConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? GuildConfig.Culture;

		public RoosterCommandContext(IMessage message, UserConfig userConfig, GuildConfig guildConfig, IServiceProvider isp) : base(isp) {
			Message = message;
			User = message.User;
			Channel = message.Channel;

			UserConfig = userConfig;
			GuildConfig = guildConfig;
		}

		public override string ToString() {
			return $"{User.Name} in channel `{Channel.Name}`: {Message.Content}";
		}
	}
}
