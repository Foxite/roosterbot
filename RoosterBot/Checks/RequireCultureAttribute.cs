using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	/// <summary>
	/// Require that <see cref="RoosterCommandContext.Culture"/> equals a certain value.
	/// </summary>
	public sealed class RequireCultureAttribute : RoosterPreconditionAttribute {
		/// 
		public override string Summary => "#RequireCultureAttribute_Summary";

		/// <summary>
		/// The <see cref="CultureInfo"/> the command is restricted to.
		/// </summary>
		public CultureInfo Culture { get; }

		/// <summary>
		/// If true and this check fails, then the fact will be hidden from the user and it will appear as though the command does not exist.
		/// If false, then the user will be informed that the command can only be used with the given culture.
		/// </summary>
		public bool Hide { get; }

		/// <param name="cultureName">The string that will be passed into <see cref="CultureInfo.GetCultureInfo(string)"/>.</param>
		/// <param name="hide">If the precondition fails, this will hide the existence of the command if this is true.</param>
		public RequireCultureAttribute(string cultureName, bool hide) {
			Culture = CultureInfo.GetCultureInfo(cultureName);
			Hide = hide;
		}

		/// 
		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (Culture == context.Culture) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.Successful);
			} else if (Hide) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#CommandHandling_NotFound", context.ChannelConfig.CommandPrefix));
			} else {
				CultureNameService cns = context.ServiceProvider.GetRequiredService<CultureNameService>();
				string localizedName = cns.GetLocalizedName(Culture, context.ChannelConfig.Culture);
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#RequireCultureAttribute_CheckFailed", localizedName));
			}
		}
	}
}
