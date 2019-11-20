using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RequireCultureAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireCultureAttribute_Summary";
		public CultureInfo Culture { get; }
		public bool Hide { get; }

		/// <param name="hide">If the precondition fails, this will hide the existence of the command if this is true.</param>
		public RequireCultureAttribute(string cultureName, bool hide) {
			Culture = CultureInfo.GetCultureInfo(cultureName);
			Hide = hide;
		}

		protected override Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, CommandInfo command, IServiceProvider services) {
			if (Culture == context.Culture) {
				return Task.FromResult(RoosterPreconditionResult.FromSuccess());
			} else if (Hide) {
				return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#Program_OnCommandExecuted_UnknownCommand", context.GuildConfig.CommandPrefix));
			} else {
				CultureNameService cns = services.GetService<CultureNameService>();
				string localizedName = cns.GetLocalizedName(Culture, context.GuildConfig.Culture);
				return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#RequireCultureAttribute_CheckFailed", localizedName));
			}
		}
	}
}
