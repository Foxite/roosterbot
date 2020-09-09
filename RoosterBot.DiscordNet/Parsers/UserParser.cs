using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.DiscordNet {
	public class UserParser<TUser> : RoosterTypeParser<TUser> where TUser : class, Discord.IUser {
		public override string TypeDisplayName => "#UserParser_Name";

		public async override ValueTask<RoosterTypeParserResult<TUser>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			Discord.IUser? user = null;

			bool parseSuccessful = false;
			if (MentionUtils.TryParseUser(input, out ulong userId) || ulong.TryParse(input, out userId)) {
				parseSuccessful = true;
				user = DiscordNetComponent.Instance.Client.GetUser(userId);
			} else {
				string[] split = input.Split('#');
				if (user == null && split.Length == 2) {
					parseSuccessful = true;
					user = DiscordNetComponent.Instance.Client.GetUser(split[0], split[1]);

					if (typeof(TUser).IsAssignableFrom(typeof(IGuildUser)) && context is DiscordCommandContext dcc && dcc.Guild != null) {
						user = await dcc.Guild.GetUserAsync(user.Id);
					}
				}
			}

			if (user == null) {
				return Unsuccessful(parseSuccessful, parseSuccessful ? "#UserParser_UnknownUser" : "#UserParser_InvalidMention");
			} else if (!(user is TUser tUser)) {
				return Unsuccessful(true, "#DiscordParser_InvalidType");
			} else {
				return Successful(tUser);
			}
		}
	}
}
