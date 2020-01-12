using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterTypeParser<T> : TypeParser<T>, IRoosterTypeParser {
		public Type Type => typeof(T);

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may be a resolvable resource.
		/// </summary>
		public abstract string TypeDisplayName { get; }

		/// <summary>
		/// Always returns an object of type ValueTask<RoosterTypeParserResult<T>>.
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public async sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await ParseAsync(parameter, value, rcc);
			} else {
				throw new InvalidOperationException($"{GetType().Name} requires a CommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			}
		}

		public abstract ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context);

		async ValueTask<IRoosterTypeParserResult> IRoosterTypeParser.ParseAsync(Parameter parameter, string value, RoosterCommandContext context) => await ParseAsync(parameter, value, context);

		protected RoosterTypeParserResult<T> Successful(T value) => RoosterTypeParserResult<T>.Successful(value);
		protected RoosterTypeParserResult<T> Unsuccessful(bool inputValid, RoosterCommandContext context, string reason, params string[] objects) {
			ResourceService Resources = context.ServiceProvider.GetService<ResourceService>();
			Component component = Program.Instance.Components.GetComponentFromAssembly(GetType().Assembly);
			return RoosterTypeParserResult<T>.Unsuccessful(inputValid, string.Format(Resources.ResolveString(context.Culture, component, reason), objects));
		}
	}
}
