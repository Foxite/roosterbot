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
			GuildConfig config = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild))!;
			if (Culture == config.Culture) {
				return PreconditionResult.FromSuccess();
			} else {
				ResourceService resources = services.GetService<ResourceService>();
				string reason;
				if (Hide) {
					reason = string.Format(resources.GetString(config.Culture, "Program_OnCommandExecuted_UnknownCommand"), config.CommandPrefix);
				} else {
					CultureNameService cns = services.GetService<CultureNameService>();
					string localizedName = cns.GetLocalizedName(Culture, config.Culture);
					reason = string.Format(resources.GetString(config.Culture, "RequireCultureAttribute_CheckFailed"), localizedName);
				}
				return PreconditionResult.FromError(reason);
			}
		}
	}
}
