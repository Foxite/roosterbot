using System;
using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Require that <see cref="RoosterCommandContext.IsPrivate"/> is equal a certain value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class RequirePrivateAttribute : RoosterPreconditionAttribute {
		/// <summary>
		/// <see cref="RoosterCommandContext.IsPrivate"/> must equal this value for this check to pass.
		/// </summary>
		public bool RequirePrivate { get; }

		/// 
		public override string Summary => RequirePrivate ? "#RequirePrivateAttribute_RequirePrivate" : "#RequirePrivateAttribute_RequirePublic";

		/// 
		public RequirePrivateAttribute(bool requirePrivate) {
			RequirePrivate = requirePrivate;
		}

		/// 
		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			return ValueTaskUtil.FromResult(
				RequirePrivate == context.IsPrivate
				? RoosterCheckResult.Successful
				: RoosterCheckResult.UnsuccessfulBuiltIn(
					RequirePrivate
					? "#RequireContextAttribute_PrivateOnly"
					: "#RequireContextAttribute_PublicOnly"
				)
			);
		}
	}
}
