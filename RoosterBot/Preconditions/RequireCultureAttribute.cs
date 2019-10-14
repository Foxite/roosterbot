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

		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			CultureInfo contextCulture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
			if (Culture == contextCulture) {
				return PreconditionResult.FromSuccess();
			} else {
				ResourceService resources = services.GetService<ResourceService>();
				string reason;
				if (Hide) {
					reason = string.Format(resources.GetString(contextCulture, "Program_OnCommandExecuted_UnknownCommand"), services.GetService<ConfigService>().DefaultCommandPrefix);
				} else {
					CultureNameService cns = services.GetService<CultureNameService>();
					string localizedName = cns.GetLocalizedName(Culture, contextCulture);
					reason = string.Format(resources.GetString(contextCulture, "RequireCultureAttribute_CheckFailed"), localizedName);
				}
				return PreconditionResult.FromError(reason);
			}
		}
	}
}
