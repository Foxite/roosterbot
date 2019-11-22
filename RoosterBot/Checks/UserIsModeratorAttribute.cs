using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context.User is IGuildUser user) {
				if (context.ServiceProvider.GetService<ConfigService>().StaffRoles.Intersect(user.RoleIds).Any()) {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromSuccess());
				} else {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_CheckFailed"));
				}
			} else {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_GuildsOnly"));
			}
		}
	}
}
