using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	public class DiscordCommandContext : RoosterCommandContext {
		public BaseSocketClient Client { get; }
		public new IUserMessage Message { get; }
		public new Discord.IUser User { get; }
		public new IMessageChannel Channel { get; }
		public new IUserMessage? Response => ((DiscordMessage?) base.Response)?.DiscordEntity;
		public new DiscordNetComponent Platform => DiscordNetComponent.Instance;
		public IGuild? Guild { get; }

		public DiscordCommandContext(IServiceProvider isp, IUserMessage message, UserConfig userConfig, ChannelConfig guildConfig)
			: base(isp, DiscordNetComponent.Instance, new DiscordMessage(message), userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message;
			User = Message.Author;
			Channel = Message.Channel;

			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}

		protected async override Task<IMessage> SendResultAsync(RoosterCommandResult result) {
			System.Collections.Generic.IEnumerable<ResultAdapter> adapters = DiscordNetComponent.Instance.GetResultAdapter(this, result);
			if (adapters.Any()) {
				return await adapters.First().HandleResult(this, result);
			} else {
				return new DiscordMessage(await Channel.SendMessageAsync("als je dit ziet mag je de developer slaan")); // TODO
			}
		}
	}
}
