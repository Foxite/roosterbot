using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		protected override Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User is IGuildUser user) {
				if (services.GetService<ConfigService>().StaffRoles.Intersect(user.RoleIds).Any()) {
					return Task.FromResult(RoosterPreconditionResult.FromSuccess());
				} else {
					return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#UserIsModeratorAttribute_CheckFailed"));
				}
			} else {
				return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#UserIsModeratorAttribute_GuildsOnly"));
			}
		}
	}
}
