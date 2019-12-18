using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class ChannelParser<TChannel> : RoosterTypeParser<TChannel> where TChannel : class, IChannel {
		public override string TypeDisplayName => "#ChannelParser_Name";

		protected override ValueTask<RoosterTypeParserResult<TChannel>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			/* // TODO Discord
			if (MentionUtils.TryParseChannel(input, out ulong channelId) || ulong.TryParse(input, out channelId)) {
				var channel = (TChannel?) await context.Client.GetChannelAsync(channelId);
				if (channel == null) {
					return Unsuccessful(true, context, "#ChannelParser_UnknownChannel");
				} else if (!(channel is TChannel)) {
					return Unsuccessful(true, context, "#DiscordParser_InvalidType");
				} else {
					return Successful(channel);
				}
			} else {
				return Unsuccessful(false, context, "#ChannelParser_InvalidMention");
			}
			*/
			throw new NotImplementedException();
		}
	}
}
