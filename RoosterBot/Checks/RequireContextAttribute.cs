using System;
using System.Threading.Tasks;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class RequireContextAttribute : RoosterPreconditionAttribute {
		public ContextType RequiredContext { get; }

		public override string Summary => throw new NotImplementedException();

		public RequireContextAttribute(ContextType requiredContext) {
			RequiredContext = requiredContext;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			string? errorReason = null;
			switch (RequiredContext) {
				case ContextType.DM:
					errorReason = context.Guild == null ? null : "#RequireContextAttribute_DMOnly";
					break;
				case ContextType.Guild:
					errorReason = context.Guild != null ? null : "#RequireContextAttribute_GuildOnly";
					break;
			}
			return new ValueTask<RoosterCheckResult>(errorReason == null ? RoosterCheckResult.Successful : RoosterCheckResult.UnsuccessfulBuiltIn(errorReason));
		}
	}

	public enum ContextType {
		DM, Guild
	}
}
