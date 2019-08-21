using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot.Preconditions {
	public class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => Resources.RequireBotManagerAttribute_Summary;

		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwner.Id) {
				return PreconditionResult.FromSuccess();
			} else {
				if (services.GetService<ConfigService>().ErrorReactions) {
					try {
						await context.Message.AddReactionAsync(new Emoji("⛔"));
					} catch (HttpException) { } // Permission denied
				}
				return PreconditionResult.FromError(Resources.RequireBotManagerAttribute_CheckFailed);
			}
		}
	}
}
