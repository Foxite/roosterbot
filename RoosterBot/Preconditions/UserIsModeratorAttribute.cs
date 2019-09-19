using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	// TODO localize
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "Je moet moderator zijn om dit te doen.";

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User is IGuildUser user) {
				if (services.GetService<ConfigService>().StaffRoles.Intersect(user.RoleIds).Any()) {
					return Task.FromResult(PreconditionResult.FromSuccess());
				} else {
					return Task.FromResult(PreconditionResult.FromError(":no_entry: Je moet moderator privileges hebben om dat te doen."));
				}
			} else {
				return Task.FromResult(PreconditionResult.FromError(":x: Dit werkt alleen in een server."));
			}
		}
	}
}
