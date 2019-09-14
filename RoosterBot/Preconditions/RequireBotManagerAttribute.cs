using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwner.Id) {
				return Task.FromResult(PreconditionResult.FromSuccess());
			} else {
				return Task.FromResult(PreconditionResult.FromError(services.GetService<ResourceService>().GetString(context, "RequireBotManagerAttribute_CheckFailed")));
			}
		}
	}
}
