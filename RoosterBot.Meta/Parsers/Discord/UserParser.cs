using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.Meta {
	public class UserParser<TUser> : RoosterTypeParser<TUser> where TUser : class, IUser {
		public override string TypeDisplayName => "#UserParser_Name";

		protected async override ValueTask<RoosterTypeParserResult<TUser>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (MentionUtils.TryParseUser(input, out ulong userId) || ulong.TryParse(input, out userId)) {
				var user = (TUser?) await context.Client.GetUserAsync(userId);
				if (user == null) {
					return Unsuccessful(true, context, "#UserParser_UnknownUser");
				} else if (!(user is TUser)) {
					return Unsuccessful(true, context, "#DiscordParser_InvalidType");
				} else {
					return Successful(user);
				}
			} else {
				return Unsuccessful(false, context, "#UserParser_InvalidMention");
			}
		}
	}
}
