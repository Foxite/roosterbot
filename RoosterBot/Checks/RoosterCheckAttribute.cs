using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all command checks used within RoosterBot. This class enforces the use of <see cref="RoosterCommandContext"/> and <see cref="RoosterCheckResult"/>.
	/// </summary>
	public abstract class RoosterCheckAttribute : CheckAttribute {
		/// <summary>
		/// If the given command context is not a RoosterCommandContext, then this indicates if an exception should be thrown, or a ParseFailed result should be returned.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		/// <summary>
		/// A user-friendly explanation for this check. It may start with a #, in which case it will be resolved as a string resource.
		/// </summary>
		public abstract string Summary { get; }

		/// 
		public async sealed override ValueTask<CheckResult> CheckAsync(CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await CheckAsync(rcc);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterCheckAttribute)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return RoosterCheckResult.UnsuccessfulBuiltIn("If you see this, then you may slap the programmer.");
			}
		}

		/// 
		protected abstract ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context);
	}
}
