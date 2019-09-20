using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class RequireCultureAttribute : RoosterPreconditionAttribute {
		public override string Summary => "Requires a {0}-speaking server."; // TODO Localize
		public CultureInfo Culture { get; }

		public RequireCultureAttribute(string cultureName) {
			Culture = CultureInfo.GetCultureInfo(cultureName);
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			var gcs = services.GetService<GuildCultureService>();
			if (gcs.GetCultureForGuild(context.Guild) == Culture) {
				return Task.FromResult(PreconditionResult.FromSuccess());
			} else {
				return Task.FromResult(PreconditionResult.FromError("This command only works in {0}-speaking servers.")); // TODO localize
			}
		}
	}
}
