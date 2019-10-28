using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwner.Id) {
				return PreconditionResult.FromSuccess();
			} else {
				CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
				return PreconditionResult.FromError(services.GetService<ResourceService>().GetString(culture, "RequireBotManagerAttribute_CheckFailed"));
			}
		}
	}
}
