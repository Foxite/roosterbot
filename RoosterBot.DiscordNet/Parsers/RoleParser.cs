using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.DiscordNet {
	public class RoleParser<TRole> : RoosterTypeParser<TRole> where TRole : class, Discord.IRole {
		public override string TypeDisplayName => "#RoleParser_Name";
		
		public override ValueTask<RoosterTypeParserResult<TRole>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (context.Channel is DiscordChannel channel) {
				if (channel.DiscordEntity is Discord.IGuildChannel igc) {
					if (Discord.MentionUtils.TryParseRole(input, out ulong roleId) || ulong.TryParse(input, out roleId)) {
						Discord.IRole role = igc.Guild.GetRole(roleId);
						if (role == null) {
							return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#RoleParser_UnknownRole"));
						} else if (!(role is TRole tRole)) {
							return ValueTaskUtil.FromResult(Unsuccessful(true, context, "#DiscordParser_InvalidType"));
						} else {
							return ValueTaskUtil.FromResult(Successful(tRole));
						}
					} else {
						return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#RoleParser_InvalidMention"));
					}
				} else {
					return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#RoleParser_GuildsOnly"));
				}
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#DiscordOnly"));
			}
		}
	}
}
