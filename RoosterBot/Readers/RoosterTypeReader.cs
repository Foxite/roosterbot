using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterTypeReader<T> : TypeParser<T> {
		/// <summary>
		/// If the given command context is not a RoosterCommandContext, then this indicates if an exception should be thrown, or a ParseFailed result should be returned.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		public Type Type => typeof(T);

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may be a resolvable resource.
		/// </summary>
		public abstract string TypeDisplayName { get; }

		public sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return ParseAsync(parameter, value, rcc);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{GetType().Name} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return new ValueTask<TypeParserResult<T>>(TypeParserResult<T>.Unsuccessful("If you see this, then you may slap the programmer."));
			}
		}

		protected abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context);
	}
}
