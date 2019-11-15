using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		protected override Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwner.Id) {
				return Task.FromResult(RoosterPreconditionResult.FromSuccess());
			} else {
				return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#RequireBotManagerAttribute_CheckFailed"));
			}
		}
	}
}
