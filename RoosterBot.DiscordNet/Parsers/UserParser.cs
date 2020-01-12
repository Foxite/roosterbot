using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.DiscordNet {
	public class UserParser<TUser> : RoosterTypeParser<TUser> where TUser : class, Discord.IUser {
		public override string TypeDisplayName => "#UserParser_Name";

		public override ValueTask<RoosterTypeParserResult<TUser>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			// TODO restrict to discord contexts
			if (Discord.MentionUtils.TryParseUser(input, out ulong userId) || ulong.TryParse(input, out userId)) {
				Discord.IUser? user = DiscordNetComponent.Instance.Client.GetUser(userId);
				if (user == null) {
					return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#UserParser_UnknownUser"));
				} else if (!(user is TUser tUser)) {
					return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#DiscordParser_InvalidType"));
				} else {
					return ValueTaskUtil.FromResult(Successful(tUser));
				}
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#UserParser_InvalidMention"));
			}
		}
	}
}
