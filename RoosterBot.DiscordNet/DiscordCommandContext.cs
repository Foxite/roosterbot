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
		public IGuild? Guild { get; }

		public DiscordCommandContext(DiscordMessage message, UserConfig userConfig, ChannelConfig guildConfig) : base(DiscordNetComponent.Instance, message, userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message.DiscordEntity;
			User = Message.Author;
			Channel = Message.Channel;
			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}

		protected async override Task<IMessage> SendResultAsync(RoosterCommandResult result) {
			var alr = result as AspectListResult ?? ((result is CompoundResult cr && cr.IndividualResults.CountEquals(1)) ? cr.IndividualResults.First() as AspectListResult : null);
			if (alr != null) {
				var embed = new EmbedBuilder()
					.WithTitle(alr.Caption)
					.WithFields(
						from aspect in alr
						select new EmbedFieldBuilder()
							.WithName(aspect.PrefixEmote.ToString() + aspect.Name)
							.WithValue(aspect.Value)
							.WithIsInline(true))
					.Build();
				if (result.UploadFilePath == null) {
					return new DiscordMessage(await Channel.SendMessageAsync(embed: embed));
				} else {
					return new DiscordMessage(await Channel.SendFileAsync(result.UploadFilePath, embed: embed));
				}
			} else {
				return await base.SendResultAsync(result);
			}
		}
	}
}
