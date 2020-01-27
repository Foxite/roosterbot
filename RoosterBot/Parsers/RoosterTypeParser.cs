using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="TypeParser{T}"/>s used within RoosterBot. This class enforces the use of <see cref="RoosterCommandContext"/> and <see cref="RoosterTypeParserResult{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of object being parsed.</typeparam>
	public abstract class RoosterTypeParser<T> : TypeParser<T>, IRoosterTypeParser {
		/// <summary>
		/// The type of object being parsed. This is equal to typeof(<typeparamref name="T"/>).
		/// </summary>
		public Type Type => typeof(T);

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may start with a #, in which case it will be resolved as a string resource.
		/// </summary>
		public abstract string TypeDisplayName { get; }

		/// <summary>
		/// Parse a string into an object of type <typeparamref name="T"/>.
		/// 
		/// This method always returns a <see cref="RoosterTypeParserResult{T}"/>.
		/// </summary>
		public async sealed override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context) {
			if (context is RoosterCommandContext rcc) {
				return await ParseAsync(parameter, value, rcc);
			} else {
				throw new InvalidOperationException($"{GetType().Name} requires a CommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			}
		}

		/// <summary>
		/// Parse a string into an object of type <typeparamref name="T"/>.
		/// </summary>
		public abstract ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context);

		async ValueTask<IRoosterTypeParserResult> IRoosterTypeParser.ParseAsync(Parameter parameter, string value, RoosterCommandContext context) => await ParseAsync(parameter, value, context);

		/// <summary>
		/// Get a successful result containing <paramref name="value"/>.
		/// </summary>
		protected RoosterTypeParserResult<T> Successful(T value) => RoosterTypeParserResult<T>.Successful(value);

		/// <summary>
		/// Get an unsuccessful result indicating if the input was valid, the context, a reason, and an optional array of objects to format <paramref name="reason"/> with.
		/// </summary>
		/// <param name="inputValid"><see langword="true"/> if the input was valid but could not be parsed due to another reason.</param>
		/// <param name="context">The <see cref="RoosterCommandContext"/> that was passed into <see cref="ParseAsync(Parameter, string, RoosterCommandContext)"/>.</param>
		/// <param name="reason">The reason for failure. This will be displayed to the user, or if it starts with a # it will be resolved and the result displayed to the user.</param>
		/// <param name="objects">Objects to format the (resolved) <paramref name="reason"/> with.</param>
		/// <returns></returns>
		protected RoosterTypeParserResult<T> Unsuccessful(bool inputValid, RoosterCommandContext context, string reason, params string[] objects) {
			ResourceService Resources = context.ServiceProvider.GetRequiredService<ResourceService>();
			if (!GetType().Assembly.Equals(Assembly.GetExecutingAssembly())) {
				Component component = Program.Instance.Components.GetComponentFromAssembly(GetType().Assembly);
				reason = Resources.ResolveString(context.Culture, component, reason);
			}
			return RoosterTypeParserResult<T>.Unsuccessful(inputValid, string.Format(reason, objects));
		}
	}
}
