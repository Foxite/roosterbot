using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// Similar to <see cref="MultiParser{T}"/>, however this class only uses one parser, and selects it based on the platform of the current context.
	/// </summary>
	public class PlatformSpecificParser<T> : RoosterTypeParser<T> {
		private readonly Dictionary<PlatformComponent, RoosterTypeParser<T>> m_Parsers;

		public override string TypeDisplayName { get; }

		public PlatformSpecificParser(string typeDisplayName) {
			TypeDisplayName = typeDisplayName;
			m_Parsers = new Dictionary<PlatformComponent, RoosterTypeParser<T>>();
		}

		public void RegisterParser(PlatformComponent platform, RoosterTypeParser<T> add) {
			m_Parsers.Add(platform, add);
		}

		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			if (m_Parsers.TryGetValue(context.Platform, out var parser)) {
				return await parser.ParseAsync(parameter, value, context); // should never throw. Also it must be awaited like this because otherwise you get conversion errors.
			} else {
				return Unsuccessful(false, context, "Type cannot be parsed under this platform"); // TODO user-friendly, localized error message
			}
		}
	}
}
