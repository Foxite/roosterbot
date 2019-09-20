using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class RequireCultureAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireCultureAttribute_Summary";
		public CultureInfo Culture { get; }
		public bool Hide { get; }

		/// <param name="hide">If the precondition fails, this will hide the existence of the command if this is true.</param>
		public RequireCultureAttribute(string cultureName, bool hide) {
			Culture = CultureInfo.GetCultureInfo(cultureName);
			Hide = hide;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			CultureInfo contextCulture = services.GetService<GuildCultureService>().GetCultureForGuild(context.Guild);
			if (Culture == contextCulture) {
				return Task.FromResult(PreconditionResult.FromSuccess());
			} else {
				ResourceService resources = services.GetService<ResourceService>();
				string reason;
				if (Hide) {
					reason = string.Format(resources.GetString(contextCulture, "Program_OnCommandExecuted_UnknownCommand"), services.GetService<ConfigService>().CommandPrefix);
				} else {
					reason = string.Format(resources.GetString(contextCulture, "RequireCultureAttribute_CheckFailed"), Culture.GetLocalizedName(contextCulture));
				}
				return Task.FromResult(PreconditionResult.FromError(reason));
			}
		}
	}
}
