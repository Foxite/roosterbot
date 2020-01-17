using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="ParameterCheckAttribute"/> within RoosterBot. This class enforces the use of <see cref="RoosterCommandContext"/>.
	/// </summary>
	public abstract class RoosterParameterCheckAttribute : ParameterCheckAttribute {
		/// <summary>
		/// If <see langword="true"/>, <see cref="CheckAsync(object, CommandContext)"/> will throw an <see cref="InvalidOperationException"/> when a context of a type other than
		/// <see cref="RoosterCommandContext"/> is received. Otherwise, it will return an unsuccessful result.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		/// <summary>
		/// Check the parameter value. <paramref name="context"/> must be a <see cref="RoosterCommandContext"/>.
		/// </summary>
		public async sealed override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await CheckAsync(argument, rcc);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterParameterCheckAttribute)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return CheckResult.Unsuccessful("If you see this, then you may slap the programmer.");
			}
		}

		/// <summary>
		/// Check the parameter value with a <see cref="RoosterCommandContext"/>.
		/// </summary>
		protected abstract ValueTask<RoosterCheckResult> CheckAsync(object argument, RoosterCommandContext context);
	}
}
