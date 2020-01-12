using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.DiscordNet {
	public class ChannelParser<TChannel> : RoosterTypeParser<TChannel> where TChannel : class, Discord.IChannel {
		public override string TypeDisplayName => "#ChannelParser_Name";

		public override ValueTask<RoosterTypeParserResult<TChannel>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (Discord.MentionUtils.TryParseChannel(input, out ulong channelId) || ulong.TryParse(input, out channelId)) {
				Discord.IChannel? channel = DiscordNetComponent.Instance.Client.GetChannel(channelId);
				if (channel == null) {
					return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#ChannelParser_UnknownChannel"));
				} else if (!(channel is TChannel tChannel)) {
					return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#DiscordParser_InvalidType"));
				} else {
					return ValueTaskUtil.FromResult(Successful(tChannel));
				}
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#ChannelParser_InvalidMention"));
			}
		}
	}
}
