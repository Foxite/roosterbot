/* // TODO Discord
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context.User is IGuildUser user) {
				//if (context.ServiceProvider.GetService<ConfigService>().StaffRoles.Intersect(user.RoleIds).Any()) {
				if (new[] {
						user.GuildPermissions.Administrator,
						user.GuildPermissions.ManageGuild,
						user.GuildPermissions.KickMembers,
						user.GuildPermissions.BanMembers
					}.Any(p => p)) {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.Successful);
				} else {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_CheckFailed"));
				}
			} else {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_GuildsOnly"));
			}
		}
	}
}
*/