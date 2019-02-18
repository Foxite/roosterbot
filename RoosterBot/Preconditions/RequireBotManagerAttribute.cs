using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot.Modules.Preconditions {
	public class RequireBotManagerAttribute : PreconditionAttribute {
		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.User.Id == services.GetService<ConfigService>().BotOwnerId) {
				return PreconditionResult.FromSuccess();
			} else {
				if (services.GetService<ConfigService>().ErrorReactions) {
					try {
						await context.Message.AddReactionAsync(new Emoji("⛔"));
					} catch (HttpException) { } // Permission denied
				}
				return PreconditionResult.FromError("Je bent niet gemachtigd om dat te doen.");
			}
		}
	}
}
