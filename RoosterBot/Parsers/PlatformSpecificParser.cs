using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// Similar to <see cref="MultiParser{T}"/>, however this class only uses one parser, and selects it based on the platform of the current context.
	/// </summary>
	public class PlatformSpecificParser<T> : RoosterTypeParser<T> {
		private readonly Dictionary<PlatformComponent, RoosterTypeParser<T>> m_Parsers;

		/// <inheritdoc/>
		public override string TypeDisplayName { get; }

		/// <inheritdoc/>
		public PlatformSpecificParser(string typeDisplayName) {
			TypeDisplayName = typeDisplayName;
			m_Parsers = new Dictionary<PlatformComponent, RoosterTypeParser<T>>();
		}

		/// <summary>
		/// Add a parser to the list of parsers to consult.
		/// </summary>
		/// <param name="platform"></param>
		/// <param name="add"></param>
		public void RegisterParser(PlatformComponent platform, RoosterTypeParser<T> add) {
			m_Parsers.Add(platform, add);
		}

		/// <inheritdoc/>
		public override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			if (m_Parsers.TryGetValue(context.Platform, out var parser)) {
				return parser.ParseAsync(parameter, value, context);
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context.Platform.Name));
			}
		}
	}
}
