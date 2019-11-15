using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		protected override Task<PreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User is IGuildUser user) {
				if (services.GetService<ConfigService>().StaffRoles.Intersect(user.RoleIds).Any()) {
					return Task.FromResult(PreconditionResult.FromSuccess());
				} else {
					return Task.FromResult(PreconditionResult.FromError("#UserIsModeratorAttribute_CheckFailed"));
				}
			} else {
				return Task.FromResult(PreconditionResult.FromError("#UserIsModeratorAttribute_GuildsOnly"));
			}
		}
	}
}
