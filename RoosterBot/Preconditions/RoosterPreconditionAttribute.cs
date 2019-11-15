using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public abstract class RoosterPreconditionAttribute : PreconditionAttribute {
		/// <summary>
		/// If the given command context is not a RoosterCommandContext, then this indicates if an exception should be thrown, or a ParseFailed result should be returned.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		public abstract string Summary { get; }

		public async sealed override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context is RoosterCommandContext rcc) {
				return await CheckPermissionsAsync(rcc, command, services);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterPreconditionAttribute)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return PreconditionResult.FromError("If you see this, then you may slap the programmer.");
			}
		}

		protected abstract Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, CommandInfo command, IServiceProvider services);
	}
}
