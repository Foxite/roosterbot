using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterParameterCheckAttribute : ParameterCheckAttribute {
		public bool ThrowOnInvalidContext { get; set; }

		public async sealed override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await CheckAsync(argument, context);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterTypeReader)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return CheckResult.Unsuccessful("If you see this, then you may slap the programmer.");
			}
		}

		protected abstract ValueTask<RoosterCheckResult> CheckAsync(object argument, RoosterCommandContext context);
	}
}
