using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public abstract class RoosterTypeReader : TypeReader {
		/// <summary>
		/// If the given command context is not a RoosterCommandContext, then this indicates if an exception should be thrown, or a ParseFailed result should be returned.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		public sealed override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			if (context is RoosterCommandContext rcc) {
				return ReadAsync(rcc, input, services);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterTypeReader)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "If you see this, then you may slap the programmer."));
			}
		}

		protected abstract Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services);
	}
}
