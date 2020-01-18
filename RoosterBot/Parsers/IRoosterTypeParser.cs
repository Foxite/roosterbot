using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// A non-generic interface for <see cref="RoosterTypeParser{T}"/>.
	/// </summary>
	public interface IRoosterTypeParser {
		/// <summary>
		/// The T in <see cref="RoosterTypeParser{T}"/>.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may be a resolvable resource.
		/// </summary>
		string TypeDisplayName { get; }

		/// <summary>
		/// A non-generic mirror for <see cref="RoosterTypeParser{T}.ParseAsync(Parameter, string, RoosterCommandContext)"/>.
		/// </summary>
		ValueTask<IRoosterTypeParserResult> ParseAsync(Parameter parameter, string value, RoosterCommandContext context);
	}
}
