using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterTypeParser<T> : TypeParser<T>, IRoosterTypeParser {
		private readonly Component m_Component;

		/// <summary>
		/// If the given command context is not a RoosterCommandContext, then this indicates if an exception should be thrown, or a ParseFailed result should be returned.
		/// </summary>
		public bool ThrowOnInvalidContext { get; set; }

		public Type Type => typeof(T);

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may be a resolvable resource.
		/// </summary>
		public abstract string TypeDisplayName { get; }

		protected RoosterTypeParser(Component component) {
			m_Component = component;
		}

		public async sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await ParseAsync(parameter, value, rcc);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{GetType().Name} requires a CommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return Unsuccessful(false, "If you see this, then you may slap the programmer.");
			}
		}

		protected abstract ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context);

		async ValueTask<IRoosterTypeParserResult> IRoosterTypeParser.ParseAsync(Parameter parameter, string value, RoosterCommandContext context) => await ParseAsync(parameter, value, context);

		protected RoosterTypeParserResult<T> Successful(T value) => RoosterTypeParserResult<T>.Successful(value);
		protected RoosterTypeParserResult<T> Unsuccessful(bool inputValid, string reason, params string[] objects) => RoosterTypeParserResult<T>.Unsuccessful(inputValid, reason, m_Component, objects);
	}
}
