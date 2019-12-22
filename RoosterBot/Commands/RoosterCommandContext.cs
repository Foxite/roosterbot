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
		public ChannelConfig ChannelConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		public RoosterCommandContext(IMessage message, UserConfig userConfig, ChannelConfig guildConfig, IServiceProvider isp) : base(isp) {
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
