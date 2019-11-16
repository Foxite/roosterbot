using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public abstract class RoosterParameterPreconditionAttribute : ParameterPreconditionAttribute {
		public bool ThrowOnInvalidContext { get; set; }

		public async sealed override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			// TODO (refactor) This reeks of repetition, see RoosterPreconditionAttribute and RoosterTypeReader
			if (context is RoosterCommandContext rcc) {
				// Do not remove the async/await declarations, otherwise you'll find out that Task<T> is not covariant and it can't convert Task<string> to Task<object>
				return await CheckPermissionsAsync(rcc, parameter, value, services);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterTypeReader)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return PreconditionResult.FromError("If you see this, then you may slap the programmer.");
			}
		}

		protected abstract Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, ParameterInfo parameter, object value, IServiceProvider services);
	}
}
