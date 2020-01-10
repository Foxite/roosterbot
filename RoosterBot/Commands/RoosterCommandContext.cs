using System;
using System.Globalization;
using Qmmands;

namespace RoosterBot {
	public class RoosterCommandContext : CommandContext {
		public PlatformComponent Platform { get; }
		public IMessage Message { get; }
		public IUser User { get; }
		public IChannel Channel { get; }
		public bool IsPrivate { get; }

		public UserConfig UserConfig { get; }
		public ChannelConfig ChannelConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		public RoosterCommandContext(PlatformComponent platform, IMessage message, UserConfig userConfig, ChannelConfig guildConfig, IServiceProvider isp) : base(isp) {
			Platform = platform;
			Message = message;
			User = message.User;
			Channel = message.Channel;

			UserConfig = userConfig;
			ChannelConfig = guildConfig;
		}

		public override string ToString() {
			return $"{User.UserName} in channel `{Channel.Name}`: {Message.Content}";
		}
	}
}
