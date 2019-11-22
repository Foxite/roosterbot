using System.Globalization;
using System.Threading.Tasks;
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

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (Culture == context.Culture) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromSuccess());
			} else if (Hide) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromErrorBuiltin("#Program_OnCommandExecuted_UnknownCommand", context.GuildConfig.CommandPrefix));
			} else {
				CultureNameService cns = context.ServiceProvider.GetService<CultureNameService>();
				string localizedName = cns.GetLocalizedName(Culture, context.GuildConfig.Culture);
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromErrorBuiltin("#RequireCultureAttribute_CheckFailed", localizedName));
			}
		}
	}
}
