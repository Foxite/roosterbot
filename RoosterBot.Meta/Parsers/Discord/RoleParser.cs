/* // TODO Discord
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.Meta {
	public class RoleParser<TRole> : RoosterTypeParser<TRole> where TRole : class, IRole {
		public override string TypeDisplayName => "#RoleParser_Name";

		protected override ValueTask<RoosterTypeParserResult<TRole>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (context.Guild != null) {
				if (MentionUtils.TryParseRole(input, out ulong roleId) || ulong.TryParse(input, out roleId)) {
					var role = (TRole?) context.Guild.GetRole(roleId);
					if (role == null) {
						return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#RoleParser_UnknownRole"));
					} else if (!(role is TRole)) {
						return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#DiscordParser_InvalidType"));
					} else {
						return ValueTaskUtil.FromResult(Successful(role));
					}
				} else {
					return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#RoleParser_InvalidMention"));
				}
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#RoleParser_GuildsOnly"));
			}
		}
	}
}
*/