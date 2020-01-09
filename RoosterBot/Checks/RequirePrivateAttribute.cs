using System;
using System.Threading.Tasks;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class RequirePrivateAttribute : RoosterPreconditionAttribute {
		public bool RequirePrivate { get; }

		public override string Summary => throw new NotImplementedException();

		public RequirePrivateAttribute(bool requirePrivate) {
			RequirePrivate = requirePrivate;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			return ValueTaskUtil.FromResult(
				RequirePrivate == context.IsPrivate
				? RoosterCheckResult.Successful
				: RoosterCheckResult.UnsuccessfulBuiltIn(
					RequirePrivate
					? "#RequireContextAttribute_DMOnly" // TODO rename keys and values
					: "#RequireContextAttribute_GuildOnly"
				)
			);
		}
	}
}
